using System.Collections.Generic;
using System.Globalization;

public static class Loc
{
    private static readonly Dictionary<string, string[]> table = new()
    {
        // Taskbar and start menu
        ["start.button"] = new[] { "пуск", "start" },
        ["start.user"] = new[] { "Глич", "Glitch" },
        ["start.task"] = new[] { "glitch.exe", "glitch.exe" },
        ["menu.continue"] = new[] { "Продолжить", "Continue" },
        ["menu.settings"] = new[] { "Настройки", "Settings" },
        ["menu.main_menu"] = new[] { "Главное меню", "Main menu" },
        ["menu.quit"] = new[] { "Выйти из игры", "Quit game" },
        ["menu.hint"] = new[] { "Esc — продолжить", "Esc — resume" },
        ["menu.paused"] = new[] { "Игра на паузе", "Game paused" },
        ["menu.my_computer"] = new[] { "Мой компьютер", "My Computer" },
        ["menu.mb"] = new[] { "Всего MB: {0}", "Total MB: {0}" },
        ["menu.run"] = new[] { "Ран №{0}", "Run #{0}" },

        // Settings window
        ["settings.title"] = new[] { "Настройки", "Settings" },
        ["settings.volume"] = new[] { "Общая громкость", "Master volume" },
        ["settings.resolution"] = new[] { "Разрешение экрана", "Screen resolution" },
        ["settings.language"] = new[] { "Язык", "Language" },
        ["settings.effects"] = new[] { "Спецэффекты экрана", "Screen effects" },
        ["settings.shake"] = new[] { "Тряска экрана", "Screen shake" },
        ["settings.on"] = new[] { "вкл", "on" },
        ["settings.off"] = new[] { "выкл", "off" },
        ["settings.ok"] = new[] { "OK", "OK" },

        // Main menu
        ["mainmenu.title"] = new[] { "Главное меню", "Main menu" },
        ["mainmenu.new_game"] = new[] { "Новая игра", "New game" },
        ["mainmenu.exit"] = new[] { "Выход", "Exit" },

        // Run HUD and popups
        ["hud.pc"] = new[] { "ПК {0}/{1} · заражено файлов {2}/{3}", "PC {0}/{1} · files infected {2}/{3}" },
        ["hud.server"] = new[] { "СЕРВЕР {0}/{1} · заражено файлов {2}/{3}", "SERVER {0}/{1} · files infected {2}/{3}" },
        ["popup.time"] = new[] { "+{0:0} с", "+{0:0} s" },
        ["popup.server_locked"] = new[] { "СЕРВЕР ЗАЩИЩЁН", "SERVER PROTECTED" },
        ["popup.need_worm"] = new[] { "нужен «Сетевой червь»", "requires “Network Worm”" },
        ["popup.combo"] = new[] { "КОМБО ×{0}!", "COMBO ×{0}!" },

        // Intro
        ["desktop.new_folder"] = new[] { "Новая папка", "New Folder" },
        ["intro.open_folder"] = new[] { "Дважды кликни по папке", "Double-click the folder" },
        ["intro.open_file"] = new[] { "Внутри что-то есть. Открой файл", "There's something inside. Open the file" },
        ["intro.feed"] = new[] { "Перетаскивай файлы в глича!", "Drag files into the glitch!" },

        // End of run screen
        ["end.window"] = new[] { "Итоги забега", "Run summary" },
        ["end.total"] = new[] { "ТОТАЛ ПОЛУЧЕНО", "TOTAL EARNED" },
        ["end.gb"] = new[] { "гб получено", "GB earned" },
        ["end.files"] = new[] { "файлов заражено", "files infected" },
        ["end.clicks"] = new[] { "кликов сделано", "clicks made" },
        ["end.computers"] = new[] { "компьютеров заражено", "computers infected" },
        ["end.record"] = new[] { "НОВЫЙ РЕКОРД!", "NEW RECORD!" },
        ["end.upgrade"] = new[] { "апгрейд", "upgrade" },
        ["end.upgrade_available"] = new[] { "апгрейд  <color=#2E9E4F>(доступно: {0})</color>", "upgrade  <color=#2E9E4F>({0} available)</color>" },
        ["end.continue"] = new[] { "продолжить", "continue" },
        ["end.comparison"] = new[] { "{0} {1}{2:0}% к прошлому рану ({3} MB)", "{0} {1}{2:0}% vs last run ({3} MB)" },
        ["end.taskbar"] = new[] { "Пауза/Настройки", "Pause/Settings" },

        // Skill tree screen
        ["tree.title"] = new[] { "ДЕРЕВО НАВЫКОВ", "SKILL TREE" },
        ["tree.hint"] = new[] { "ЛКМ по узлу — купить   •   Тянуть мышью — двигать   •   Колёсико — зум", "LMB on a node — buy   •   Drag — move   •   Wheel — zoom" },
        ["tree.new_run"] = new[] { "Новый забег", "New run" },
        ["tree.soon"] = new[] { "скоро", "soon" },
        ["tooltip.soon"] = new[] { "Скоро", "Soon" },
        ["tooltip.level_max"] = new[] { "Уровень {0}/{1} — <color=#FFD24D>максимум</color>", "Level {0}/{1} — <color=#FFD24D>max</color>" },
        ["tooltip.level_cost"] = new[] { "Уровень {0}/{1}\nЦена: <color={2}>{3} MB</color>", "Level {0}/{1}\nCost: <color={2}>{3} MB</color>" },
        ["tooltip.mastery"] = new[] { "Прокачано дерево: <color={0}>{1}/{2}</color>", "Tree mastered: <color={0}>{1}/{2}</color>" },
        ["deny.soon"] = new[] { "Скоро", "Soon" },
        ["deny.max"] = new[] { "Максимум", "Maxed" },
        ["deny.need_all"] = new[] { "Нужно всё дерево", "Needs the full tree" },
        ["deny.no_mb"] = new[] { "Мало MB", "Not enough MB" },
        ["deny.locked"] = new[] { "Ещё закрыт", "Still locked" },

        // Skill nodes
        ["skill.core.name"] = new[] { "Вирус", "Virus" },
        ["skill.core.desc"] = new[] { "Точка заражения. Отсюда растут все ветки.", "Patient zero. Every branch grows from here." },
        ["skill.spawn_rate_1.name"] = new[] { "Скорость", "Speed" },
        ["skill.spawn_rate_1.desc"] = new[] { "Файлы появляются на рабочем столе чаще.", "Files appear on the desktop more often." },
        ["skill.max_files.name"] = new[] { "Захламлённый стол", "Cluttered Desktop" },
        ["skill.max_files.desc"] = new[] { "На экране помещается больше файлов.", "More files fit on the screen." },
        ["skill.start_files.name"] = new[] { "Автозагрузка", "Startup Items" },
        ["skill.start_files.desc"] = new[] { "Забег начинается с кучей файлов на столе.", "Runs start with a pile of files on the desktop." },
        ["skill.spawn_burst.name"] = new[] { "Разгон", "Overclock" },
        ["skill.spawn_burst.desc"] = new[] { "Файлы появляются пачками: +1 файл в каждой пачке.", "Files spawn in batches: +1 file per batch." },
        ["skill.file_value.name"] = new[] { "Сжатие", "Compression" },
        ["skill.file_value.desc"] = new[] { "Каждый съеденный файл приносит больше MB.", "Every eaten file gives more MB." },
        ["skill.golden.name"] = new[] { "Ценные файлы", "Valuable Files" },
        ["skill.golden.desc"] = new[] { "Иногда появляются золотые файлы: они стоят в 5 раз больше.", "Golden files sometimes appear: they are worth 5 times more." },
        ["skill.attraction.name"] = new[] { "Автономность", "Autonomy" },
        ["skill.attraction.desc"] = new[] { "Вирус сам притягивает и съедает файлы вокруг себя.", "The virus pulls in and eats nearby files on its own." },
        ["skill.attraction_radius.name"] = new[] { "Широкий охват", "Wide Reach" },
        ["skill.attraction_radius.desc"] = new[] { "Вирус притягивает файлы с большего расстояния.", "The virus pulls files from farther away." },
        ["skill.pull_speed.name"] = new[] { "Сильная гравитация", "Strong Gravity" },
        ["skill.pull_speed.desc"] = new[] { "Файлы летят к вирусу быстрее.", "Files fly to the virus faster." },
        ["skill.screen_wide.name"] = new[] { "Охота", "Hunt" },
        ["skill.screen_wide.desc"] = new[] { "Вирус сам выбирает файлы по всему экрану и затягивает их. Каждый уровень охотится чаще, с 3-го — по несколько файлов сразу.", "The virus picks files anywhere on screen and drags them in. Each level hunts more often; from level 3 it grabs several at once." },
        ["skill.run_time.name"] = new[] { "Отсрочка увольнения", "Delayed Firing" },
        ["skill.run_time.desc"] = new[] { "Забег длится дольше.", "Runs last longer." },
        ["skill.run_time_2.name"] = new[] { "Больничный", "Sick Leave" },
        ["skill.run_time_2.desc"] = new[] { "Забег длится намного дольше.", "Runs last much longer." },
        ["skill.server_access.name"] = new[] { "Сетевой червь", "Network Worm" },
        ["skill.server_access.desc"] = new[] { "Последний, 11-й компьютер — сервер фирмы. Он защищён, пока вирус не освоит всё. Прокачай всё дерево до максимума.", "The last, 11th computer is the company server. It stays protected until the virus has mastered everything. Max out the whole tree." },
        ["skill.cursor_radius.name"] = new[] { "Цепкий курсор", "Sticky Cursor" },
        ["skill.cursor_radius.desc"] = new[] { "Тащишь один файл — курсор прихватывает и все файлы рядом.", "Drag one file and the cursor grabs every file nearby too." },
        ["skill.hover_feed.name"] = new[] { "Без рук", "Hands-Free" },
        ["skill.hover_feed.desc"] = new[] { "Кнопку зажимать больше не нужно: просто води мышкой, файлы липнут к курсору. Заведи их в глича.", "No more holding the button: just move the mouse, files stick to the cursor. Lead them into the glitch." },
        ["skill.mutation.name"] = new[] { "Мутирование", "Mutation" },
        ["skill.mutation.desc"] = new[] { "Вирус мутирует. Ешь файлы без пауз — растёт комбо, и каждый файл в серии стоит больше.", "The virus mutates. Eat files without pauses to build a combo: each file in the streak is worth more." },
        ["skill.gluttony.name"] = new[] { "Ненасытность", "Gluttony" },
        ["skill.gluttony.desc"] = new[] { "Каждый шаг комбо даёт ещё больше MB.", "Each combo step gives even more MB." },
        ["skill.chain.name"] = new[] { "Цепная реакция", "Chain Reaction" },
        ["skill.chain.desc"] = new[] { "Съеденный файл может заразить другой файл — тот сам летит в глича.", "An eaten file can infect another one, which then flies into the glitch on its own." },
        ["skill.expansion.name"] = new[] { "Экспансия", "Expansion" },
        ["skill.expansion.desc"] = new[] { "Каждый заражённый компьютер добавляет время к забегу.", "Each infected computer adds time to the run." },
        ["skill.satellites.name"] = new[] { "Отпочкование", "Budding" },
        ["skill.satellites.desc"] = new[] { "Вокруг глича кружат его маленькие копии и съедают всё, к чему прикоснутся.", "Tiny copies of the glitch orbit around it and eat everything they touch." },

        // Skill stats
        ["stat.spawn_rate"] = new[] { "частота файлов ×{0:0.##}", "file spawn rate ×{0:0.##}" },
        ["stat.max_files"] = new[] { "+{0:0} к лимиту файлов", "+{0:0} file limit" },
        ["stat.start_files"] = new[] { "+{0:0} файлов на старте", "+{0:0} files at start" },
        ["stat.attraction_radius"] = new[] { "радиус притяжения {0:0.#}", "pull radius {0:0.#}" },
        ["stat.pull_speed"] = new[] { "+{0:0.#} к скорости притяжения", "+{0:0.#} pull speed" },
        ["stat.screen_wide"] = new[] { "охота, уровень {0:0}", "hunt, level {0:0}" },
        ["stat.run_time"] = new[] { "+{0:0} с к забегу", "+{0:0} s per run" },
        ["stat.file_value"] = new[] { "{0:0} MB за файл", "{0:0} MB per file" },
        ["stat.spawn_burst"] = new[] { "{0:0} файла в пачке", "{0:0} files per batch" },
        ["stat.golden"] = new[] { "{0:0}% золотых файлов", "{0:0}% golden files" },
        ["stat.cursor_radius"] = new[] { "радиус курсора {0:0.##}", "cursor radius {0:0.##}" },
        ["stat.hover_on"] = new[] { "кормление наведением", "hover feeding" },
        ["stat.hover_off"] = new[] { "только перетаскивание", "drag only" },
        ["stat.combo"] = new[] { "+{0:0}% MB за шаг комбо", "+{0:0}% MB per combo step" },
        ["stat.chain"] = new[] { "{0:0}% шанс цепной реакции", "{0:0}% chain reaction chance" },
        ["stat.satellites"] = new[] { "{0:0} копий вокруг глича", "{0:0} copies around the glitch" },
        ["stat.time_per_pc"] = new[] { "+{0:0} с за каждый ПК", "+{0:0} s per computer" },
        ["stat.server_open"] = new[] { "сервер открыт", "server unlocked" },
        ["stat.server_closed"] = new[] { "сервер закрыт", "server locked" },

        // Ending
        ["ending.glitch_words"] = new[]
        {
            "ERROR|FATAL|0xDEAD|0xBEEF|SYSTEM32 удалён|ДОСТУП ЗАПРЕЩЁН|%$#@!|заражено|ПК 11/11|format C:|NOT RESPONDING|глич.exe|kernel32.dll|не выключайте компьютер",
            "ERROR|FATAL|0xDEAD|0xBEEF|SYSTEM32 deleted|ACCESS DENIED|%$#@!|infected|PC 11/11|format C:|NOT RESPONDING|glitch.exe|kernel32.dll|do not turn off your computer"
        },
        ["ending.bsod"] = new[]
        {
            "Обнаружена неустранимая ошибка. Windows была остановлена, чтобы предотвратить повреждение компьютера.\n\nGLITCH_ATE_EVERYTHING\n\nЗаражено компьютеров: {0} из {0}\nСервер фирмы: ЗАРАЖЁН\nВаши файлы: съедены\nВаша должность: сокращена\n\nЕсли вы видите этот экран впервые, перезагрузите компьютер. Не поможет.\n\nТехнические сведения:\n*** STOP: 0x0000000B (0x0000DEAD, 0x0000BEEF, 0x00000011, 0x6C1C4000)\n\nНажмите любую клавишу...",
            "A fatal exception has occurred. Windows has been shut down to prevent damage to your computer.\n\nGLITCH_ATE_EVERYTHING\n\nComputers infected: {0} of {0}\nCompany server: INFECTED\nYour files: eaten\nYour position: eliminated\n\nIf this is the first time you've seen this stop error screen, restart your computer. It won't help.\n\nTechnical information:\n*** STOP: 0x0000000B (0x0000DEAD, 0x0000BEEF, 0x00000011, 0x6C1C4000)\n\nPress any key..."
        },
        ["ending.title"] = new[] { "ВСЕ {0} КОМПЬЮТЕРОВ ЗАРАЖЕНЫ", "ALL {0} COMPUTERS INFECTED" },
        ["ending.subtitle"] = new[] { "Глич сожрал весь офис. Тебя, кстати, уволили.", "The glitch ate the whole office. You're fired, by the way." },
        ["ending.stats"] = new[]
        {
            "Ранов: {0}   ·   MB за всё время: {1}   ·   Файлов съедено: {2}   ·   Кликов: {3}",
            "Runs: {0}   ·   All-time MB: {1}   ·   Files eaten: {2}   ·   Clicks: {3}"
        },
        ["ending.restart"] = new[] { "Начать заново", "Start over" },
        ["ending.keep_eating"] = new[] { "Продолжить жрать", "Keep eating" },
    };

    public static string T(string key)
    {
        return TryGet(key, out string value) ? value : key;
    }

    public static string F(string key, params object[] args)
    {
        return string.Format(CultureInfo.InvariantCulture, T(key), args);
    }

    public static bool TryGet(string key, out string value)
    {
        if (key != null && table.TryGetValue(key, out string[] values))
        {
            int index = (int)GameSettings.Language;
            value = index < values.Length ? values[index] : values[0];
            return true;
        }

        value = null;
        return false;
    }
}
