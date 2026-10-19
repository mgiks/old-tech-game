using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class IcqStory : MonoBehaviour, IPointerClickHandler
{
    private const float RowHeight = 52f;
    private const float BubbleWidth = 460f;

    private static readonly string[] NameKeys =
    {
        "icq.boss",
        "icq.martin",
        "icq.vitas",
        "icq.xavier",
        "icq.katie",
        "icq.sema"
    };

    private IcqArt art;
    private Image[] rows;
    private Image bossNotify;
    private RectTransform log;
    private float logHeight;
    private TextMeshProUGUI chatTitle;
    private int openContact = -1;
    private bool advance;
    private float acceptClickAt;
    private Action finished;

    public static void Play(Action done)
    {
        GameObject go = new GameObject("ICQ Story", typeof(RectTransform));
        IcqStory story = go.AddComponent<IcqStory>();
        story.finished = done;
        story.Begin();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Time.unscaledTime < acceptClickAt)
        {
            return;
        }

        advance = true;
    }

    private void Begin()
    {
        art = Resources.Load<IcqArt>("IcqArt");
        BuildCanvas();
        BuildWindows();
        StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        yield return StartCoroutine(Pause(0.45f));
        OpenContact(0);
        Incoming("icq.boss", "icq.boss_message");
        yield return StartCoroutine(Pause(3.2f));
        Outgoing("icq.player_reply");
        yield return StartCoroutine(Pause(3.2f));
        OpenContact(5);
        Outgoing("icq.sema_message");
        yield return StartCoroutine(Pause(6.5f));
        Action done = finished;
        Destroy(gameObject);
        done?.Invoke();
    }

    private IEnumerator Pause(float seconds)
    {
        float wake = Time.realtimeSinceStartup + seconds;
        acceptClickAt = Time.unscaledTime + 0.4f;
        advance = false;
        while (Time.realtimeSinceStartup < wake && !advance)
        {
            if (GamePause.IsPaused)
            {
                wake += Time.unscaledDeltaTime;
            }

            yield return null;
        }
    }

    private void Incoming(string nameKey, string bodyKey)
    {
        GameAudio.Stop(GameSfx.Icq);
        GameAudio.Play(GameSfx.Icq);
        if (bossNotify != null)
        {
            bossNotify.enabled = false;
        }

        AddBubble(true, Loc.T(nameKey), Loc.T(bodyKey));
    }

    private void Outgoing(string bodyKey)
    {
        AddBubble(false, Loc.T("icq.me"), Loc.T(bodyKey));
    }

    private void OpenContact(int index)
    {
        openContact = index;
        if (chatTitle != null)
        {
            chatTitle.text = Loc.T(NameKeys[index]);
        }

        for (int i = 0; i < rows.Length; i++)
        {
            if (rows[i] == null || art == null)
            {
                continue;
            }

            Sprite row = i == index ? art.rowSelected : art.rowNormal;
            if (row != null)
            {
                rows[i].sprite = row;
            }
        }

        ClearLog();
    }

    private void BuildCanvas()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 400;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 1f;

        gameObject.AddComponent<GraphicRaycaster>();
    }

    private void BuildWindows()
    {
        RectTransform list = Window("Contacts", new Vector2(-470f, 24f), new Vector2(440f, 640f), "icq");
        RectTransform chat = Window("Chat", new Vector2(280f, 24f), new Vector2(740f, 640f), "icq.chat");
        chatTitle = chat.GetComponentInChildren<TextMeshProUGUI>();

        RectTransform listBody = Body(list);
        rows = new Image[NameKeys.Length];
        Sprite[] avatars =
        {
            null,
            art != null ? art.martin : null,
            art != null ? art.vitas : null,
            art != null ? art.xavier : null,
            art != null ? art.katie : null,
            art != null ? art.sema : null
        };

        for (int i = 0; i < NameKeys.Length; i++)
        {
            rows[i] = ContactRow(listBody, i, avatars[i]);
        }

        bossNotify = Notify(rows[0].transform);

        RectTransform chatBody = Body(chat);
        log = XpUi.Rect("Log", chatBody);
        XpUi.Stretch(log, 16f, 72f, 16f, 12f);
        log.gameObject.AddComponent<RectMask2D>();

        Image field = Slice("Input", chatBody, art != null ? art.input : null, XpUi.Hex("#FFFFFF"));
        XpUi.Place(field.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(16f, 16f), new Vector2(620f, 44f));
        Image send = Slice("Send", chatBody, art != null ? art.send : null, XpUi.Hex("#3D95FF"));
        XpUi.Place(send.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-16f, 16f), new Vector2(110f, 44f));
        TextMeshProUGUI sendLabel = XpUi.Text("Send Label", send.transform, Loc.T("icq.send"), 18f, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
        XpUi.Stretch(sendLabel.rectTransform);

        Image catcher = XpUi.Image("Advance", transform, null, new Color(0f, 0f, 0f, 0f), true);
        XpUi.Stretch(catcher.rectTransform);
        catcher.transform.SetAsLastSibling();
        catcher.gameObject.AddComponent<ForwardClick>().Bind(this);
    }

    private RectTransform Window(string name, Vector2 position, Vector2 size, string titleKey)
    {
        Image frame = Slice(name, transform, art != null ? art.frame : null, XpUi.WindowFrame);
        XpUi.Place(frame.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, size);

        Image title = Slice("Title", frame.transform, art != null ? art.titlebar : null, XpUi.Hex("#0A5FEA"));
        title.rectTransform.anchorMin = new Vector2(0f, 1f);
        title.rectTransform.anchorMax = Vector2.one;
        title.rectTransform.pivot = new Vector2(0.5f, 1f);
        title.rectTransform.anchoredPosition = Vector2.zero;
        title.rectTransform.sizeDelta = new Vector2(-8f, 34f);

        TextMeshProUGUI label = XpUi.Text("Title Text", title.transform, Loc.T(titleKey), 22f, Color.white, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
        XpUi.Stretch(label.rectTransform, 12f, 0f, 12f, 0f);
        return frame.rectTransform;
    }

    private RectTransform Body(RectTransform window)
    {
        Image body = XpUi.Image("Body", window, null, XpUi.Hex("#F4F1E4"));
        XpUi.Stretch(body.rectTransform, 6f, 6f, 6f, 40f);
        return body.rectTransform;
    }

    private Image ContactRow(RectTransform parent, int index, Sprite avatar)
    {
        Image row = Slice("Contact " + index, parent, art != null ? art.rowNormal : null, XpUi.Hex("#EFEBD8"));
        XpUi.Place(row.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(8f, -8f - index * (RowHeight + 4f)), new Vector2(-16f, RowHeight));
        row.rectTransform.anchorMax = new Vector2(1f, 1f);
        row.rectTransform.sizeDelta = new Vector2(-16f, RowHeight);

        Image status = XpUi.Image("Status", row.transform, art != null ? art.statusOnline : null, Color.white);
        XpUi.Place(status.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(10f, 0f), new Vector2(14f, 14f));
        status.preserveAspect = true;

        if (avatar != null)
        {
            Image face = XpUi.Image("Avatar", row.transform, avatar, Color.white);
            XpUi.Place(face.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(30f, 0f), new Vector2(36f, 36f));
            face.preserveAspect = true;
        }

        float textLeft = avatar != null ? 74f : 32f;
        TextMeshProUGUI name = XpUi.Text("Name", row.transform, Loc.T(NameKeys[index]), 16f, XpUi.TextDark, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
        name.overflowMode = TextOverflowModes.Ellipsis;
        name.textWrappingMode = TextWrappingModes.NoWrap;
        XpUi.Stretch(name.rectTransform, textLeft, 4f, 28f, 4f);
        return row;
    }

    private Image Notify(Transform row)
    {
        Image mark = XpUi.Image("Notify", row, art != null ? art.notify : null, new Color(0.85f, 0.15f, 0.15f, 1f));
        XpUi.Place(mark.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-16f, 0f), new Vector2(16f, 16f));
        mark.preserveAspect = true;
        return mark;
    }

    private void AddBubble(bool incoming, string name, string body)
    {
        RectTransform row = XpUi.Rect(incoming ? "In" : "Out", log);
        float y = -logHeight;
        TextMeshProUGUI caption = XpUi.Text("Who", row, name, 16f, XpUi.TextDark, incoming ? TextAlignmentOptions.MidlineLeft : TextAlignmentOptions.MidlineRight, FontStyles.Bold);
        caption.rectTransform.anchorMin = new Vector2(0f, 1f);
        caption.rectTransform.anchorMax = new Vector2(1f, 1f);
        caption.rectTransform.pivot = new Vector2(0.5f, 1f);
        caption.rectTransform.anchoredPosition = new Vector2(0f, 0f);
        caption.rectTransform.sizeDelta = new Vector2(-8f, 22f);

        Sprite bubbleSprite = art == null ? null : incoming ? art.bubbleIn : art.bubbleOut;
        Image bubble = Slice("Bubble", row, bubbleSprite, Color.white);
        TextMeshProUGUI text = XpUi.Text("Body", bubble.transform, body, 22f, XpUi.TextDark, TextAlignmentOptions.TopLeft);
        text.textWrappingMode = TextWrappingModes.Normal;
        Vector2 preferred = text.GetPreferredValues(body, BubbleWidth - 28f, 0f);
        float height = Mathf.Max(52f, preferred.y + 28f);

        bubble.rectTransform.anchorMin = new Vector2(incoming ? 0f : 1f, 1f);
        bubble.rectTransform.anchorMax = new Vector2(incoming ? 0f : 1f, 1f);
        bubble.rectTransform.pivot = new Vector2(incoming ? 0f : 1f, 1f);
        bubble.rectTransform.anchoredPosition = new Vector2(incoming ? 4f : -4f, -24f);
        bubble.rectTransform.sizeDelta = new Vector2(BubbleWidth, height);
        XpUi.Stretch(text.rectTransform, 14f, 10f, 14f, 10f);

        row.anchorMin = new Vector2(0f, 1f);
        row.anchorMax = new Vector2(1f, 1f);
        row.pivot = new Vector2(0.5f, 1f);
        row.anchoredPosition = new Vector2(0f, y);
        row.sizeDelta = new Vector2(0f, height + 32f);
        logHeight += height + 40f;
    }

    private void ClearLog()
    {
        for (int i = log.childCount - 1; i >= 0; i--)
        {
            Destroy(log.GetChild(i).gameObject);
        }

        logHeight = 0f;
    }

    private Image Slice(string name, Transform parent, Sprite sprite, Color fallback)
    {
        Image image = XpUi.Image(name, parent, sprite, Color.white);
        if (sprite == null)
        {
            image.color = fallback;
            return image;
        }

        image.type = Image.Type.Sliced;
        if (sprite.pixelsPerUnit > 0f)
        {
            image.pixelsPerUnitMultiplier = 100f / sprite.pixelsPerUnit;
        }

        return image;
    }

    private sealed class ForwardClick : MonoBehaviour, IPointerClickHandler
    {
        private IcqStory story;

        public void Bind(IcqStory owner)
        {
            story = owner;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (story != null)
            {
                story.OnPointerClick(eventData);
            }
        }
    }
}
