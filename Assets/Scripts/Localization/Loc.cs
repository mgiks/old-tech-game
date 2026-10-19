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
        ["popup.time"] = new[] { "+{0:0.#} с", "+{0:0.#} s" },
        ["popup.server_locked"] = new[] { "СЕРВЕР ЗАЩИЩЁН", "SERVER PROTECTED" },
        ["popup.need_tree"] = new[] { "сначала всё дерево", "max out the skill tree" },

        // Intro
        ["desktop.new_folder"] = new[] { "Новая папка", "New Folder" },
        ["intro.open_folder"] = new[] { "Дважды кликни по папке", "Double-click the folder" },
        ["intro.open_file"] = new[] { "Внутри что-то есть. Открой файл", "There's something inside. Open the file" },
        ["intro.feed"] = new[] { "Перетаскивай файлы в глича!", "Drag files into the glitch!" },

        // End of run screen
        ["end.window"] = new[] { "Итоги забега", "Run summary" },
        ["end.total"] = new[] { "ТОТАЛ ПОЛУЧЕНО", "TOTAL EARNED" },
        ["end.gb"] = new[] { "гб получено", "GB earned" },
        ["end.files"] = new[] { "Файлов заражено", "Files infected" },
        ["end.clicks"] = new[] { "Кликов сделано", "Clicks made" },
        ["end.computers"] = new[] { "Компьютеров заражено", "Computers infected" },
        ["end.record"] = new[] { "НОВЫЙ РЕКОРД!", "NEW RECORD!" },
        ["end.upgrade"] = new[] { "Апгрейд", "Upgrade" },
        ["end.upgrade_available"] = new[] { "Aпгрейд  <color=#2E9E4F>(доступно: {0})</color>", "Upgrade  <color=#2E9E4F>({0} available)</color>" },
        ["end.continue"] = new[] { "Продолжить", "Continue" },
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
        ["skill.confidential.name"] = new[] { "Секретные файлы", "Confidential Files" },
        ["skill.confidential.desc"] = new[] { "Редко появляются секретные документы. Они стоят в 25 раз больше обычного файла.", "Confidential documents sometimes appear. They are worth 25 times a normal file." },
        ["skill.crawler.name"] = new[] { "Ползун", "Crawler" },
        ["skill.crawler.desc"] = new[] { "Маленький вирус сам ползёт к свободному файлу, садится на него и скачивает. Файл приносит MB, как будто его съел глич.", "A tiny virus crawls to a free file, sits on it and downloads. The file pays MB as if the glitch ate it." },
        ["skill.crawler_speed.name"] = new[] { "Быстрые лапки", "Quick Feet" },
        ["skill.crawler_speed.desc"] = new[] { "Ползун быстрее добирается до файла.", "The crawler reaches files faster." },
        ["skill.crawler_download.name"] = new[] { "Быстрая закачка", "Fast Download" },
        ["skill.crawler_download.desc"] = new[] { "Ползун меньше сидит на файле. Сначала закачка занимает 1,7 с, на максимуме около 0,5 с.", "The crawler sits on a file for less time. Download starts at 1.7 s and reaches about 0.5 s at max." },
        ["skill.colony.name"] = new[] { "Колония", "Colony" },
        ["skill.colony.desc"] = new[] { "Ещё один ползун. Вместе с первым их не больше трёх, и они не бегут на один и тот же файл.", "Another crawler. With the first one there are never more than three, and they never chase the same file." },
        ["skill.instinct.name"] = new[] { "Острый нюх", "Sharp Instinct" },
        ["skill.instinct.desc"] = new[] { "Ползуны сначала идут за золотыми и секретными файлами, но не бросают близкий обычный файл ради далёкого.", "Crawlers prefer golden and confidential files, but will not abandon a nearby normal file for a distant one." },
        ["skill.run_time.name"] = new[] { "Отсрочка увольнения", "Delayed Firing" },
        ["skill.run_time.desc"] = new[] { "Забег длится дольше.", "Runs last longer." },
        ["skill.run_time_2.name"] = new[] { "Больничный", "Sick Leave" },
        ["skill.run_time_2.desc"] = new[] { "Забег длится намного дольше.", "Runs last much longer." },
        ["skill.smoke_break.name"] = new[] { "Перекур", "Smoke Break" },
        ["skill.smoke_break.desc"] = new[] { "При заражении следующего компьютера к таймеру добавляется секунда. Один раз за переход.", "Infecting the next computer adds one second. Once per transition." },
        ["skill.overtime.name"] = new[] { "Сверхурочные", "Overtime" },
        ["skill.overtime.desc"] = new[] { "Каждые 100 MB, заработанные за этот забег, добавляют немного времени. Один и тот же порог не срабатывает дважды.", "Every 100 MB earned this run adds a little time. Each threshold pays only once." },
        ["skill.sabotage.name"] = new[] { "Саботаж", "Sabotage" },
        ["skill.sabotage.desc"] = new[] { "На 3, 6 и 9 заражённом компьютере таймер получает добавку. Каждый порог срабатывает один раз.", "Computers 3, 6 and 9 restore some of the timer. Each threshold pays only once." },
        ["skill.cursor_radius.name"] = new[] { "Цепкий курсор", "Sticky Cursor" },
        ["skill.cursor_radius.desc"] = new[] { "Тащишь один файл — курсор прихватывает и все файлы рядом.", "Drag one file and the cursor grabs every file nearby too." },
        ["skill.magnetic.name"] = new[] { "Магнитный курсор", "Magnetic Cursor" },
        ["skill.magnetic.desc"] = new[] { "Файлы в небольшом радиусе плавно тянутся к курсору.", "Files within a small radius drift smoothly toward the cursor." },
        ["skill.wide_grip.name"] = new[] { "Широкий хват", "Wide Grip" },
        ["skill.wide_grip.desc"] = new[] { "Курсор держит больше файлов сразу. Каждый уровень добавляет два места.", "The cursor holds more files at once. Each level adds two slots." },
        ["skill.hover_feed.name"] = new[] { "Без рук", "Hands-Free" },
        ["skill.hover_feed.desc"] = new[] { "Кнопку зажимать больше не нужно: просто води мышкой, файлы липнут к курсору. Заведи их в глича.", "No more holding the button: just move the mouse, files stick to the cursor. Lead them into the glitch." },
        ["skill.vacuum.name"] = new[] { "Пылесос", "Vacuum" },
        ["skill.vacuum.desc"] = new[] { "Задержи курсор на месте — вокруг него открывается зона, и файлы сами слетаются и съедаются.", "Hold the cursor still and an area opens around it. Nearby files fly in and get eaten." },

        // Skill stats
        ["stat.spawn_rate"] = new[] { "частота файлов ×{0:0.##}", "file spawn rate ×{0:0.##}" },
        ["stat.max_files"] = new[] { "+{0:0} к лимиту файлов", "+{0:0} file limit" },
        ["stat.start_files"] = new[] { "+{0:0} файлов на старте", "+{0:0} files at start" },
        ["stat.run_time"] = new[] { "+{0:0} с к забегу", "+{0:0} s per run" },
        ["stat.file_value"] = new[] { "{0:0} MB за файл", "{0:0} MB per file" },
        ["stat.spawn_burst"] = new[] { "{0:0} файла в пачке", "{0:0} files per batch" },
        ["stat.golden"] = new[] { "{0:0}% золотых файлов", "{0:0}% golden files" },
        ["stat.cursor_radius"] = new[] { "радиус курсора {0:0.##}", "cursor radius {0:0.##}" },
        ["stat.hover_on"] = new[] { "кормление наведением", "hover feeding" },
        ["stat.hover_off"] = new[] { "только перетаскивание", "drag only" },
        ["stat.crawlers"] = new[] { "ползунов: {0:0}", "crawlers: {0:0}" },
        ["stat.crawler_speed"] = new[] { "{0:0.#} ед./с", "{0:0.#} u/s" },
        ["stat.crawler_download"] = new[] { "закачка {0:0.##} с", "download {0:0.##} s" },
        ["stat.confidential"] = new[] { "{0:0.#}% секретных файлов", "{0:0.#}% confidential files" },
        ["stat.instinct_on"] = new[] { "ползуны чуют редкие файлы", "crawlers sniff out rare files" },
        ["stat.instinct_off"] = new[] { "ползуны берут ближайший файл", "crawlers take the nearest file" },
        ["stat.magnetic"] = new[] { "магнит {0:0.#}", "magnet {0:0.#}" },
        ["stat.grip"] = new[] { "в руке до {0:0} файлов", "hold up to {0:0} files" },
        ["stat.vacuum_on"] = new[] { "зона пылесоса", "vacuum area" },
        ["stat.vacuum_off"] = new[] { "пылесос выключен", "vacuum off" },
        ["stat.smoke"] = new[] { "+{0:0} с за переход на ПК", "+{0:0} s per PC jump" },
        ["stat.overtime"] = new[] { "+{0:0.#} с за 100 MB", "+{0:0.#} s per 100 MB" },
        ["stat.sabotage"] = new[] { "+{0:0} с на 3, 6 и 9 ПК", "+{0:0} s at PCs 3, 6 and 9" },

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
