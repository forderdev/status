using System.Collections.Generic;
using System.ComponentModel;

namespace PlayerStatus
{
    public class Config
    {
        [Description("Maximum status length in characters.")]
        public int MaxLength { get; set; } = 72;

        [Description("Statuses longer than this are split into lines at word boundaries.")]
        public int MaxCharsPerLine { get; set; } = 28;

        [Description("Maximum number of lines. Text that still does not fit is shrunk.")]
        public int MaxLines { get; set; } = 2;

        [Description("Smallest size a long status can be shrunk to, in percent.")]
        public int MinTextSize { get; set; } = 60;

        [Description("Allow players to use rich text tags such as <color> or <size>.")]
        public bool AllowRichText { get; set; } = false;

        [Description("Text format. {status} is replaced with the player's status.")]
        public string TextFormat { get; set; } = "{status}";

        [Description("Arguments that remove the status, e.g. '.status sil'.")]
        public List<string> ClearKeywords { get; set; } = new List<string> { "sil", "kaldır", "kaldir", "temizle", "clear", "off", "remove", "-" };

        [Description("Seconds a player has to wait between two status changes.")]
        public float Cooldown { get; set; } = 2f;

        [Description("Remove the status when the player's role changes.")]
        public bool ClearOnRoleChange { get; set; } = true;

        [Description("Height of the text above the player's eyes, in metres.")]
        public float HeightAboveHead { get; set; } = 0.3f;

        [Description("Text scale.")]
        public float Scale { get; set; } = 0.12f;

        [Description("Text box width. Keep it wide so lines are not wrapped a second time.")]
        public float DisplayWidth { get; set; } = 1000f;

        [Description("Text box height.")]
        public float DisplayHeight { get; set; } = 50f;

        [Description("Billboard: the text turns towards every viewer. Front: the text faces the player's front.")]
        public string Mode { get; set; } = "Billboard";

        [Description("Maximum distance the text can be seen from, in metres.")]
        public float MaxViewDistance { get; set; } = 12f;

        [Description("Hide the text behind walls and closed doors.")]
        public bool RequireLineOfSight { get; set; } = true;

        [Description("Hide the text of players the viewer cannot see (SCP-268 and similar).")]
        public bool RespectInvisibility { get; set; } = true;

        [Description("Show players their own status.")]
        public bool ShowToSelf { get; set; } = false;

        [Description("Let spectators see statuses through the player they spectate.")]
        public bool ShowToSpectators { get; set; } = true;

        [Description("Visibility and rotation updates per second.")]
        public int UpdateRate { get; set; } = 15;

        [Description("Write status changes to the server console.")]
        public bool LogChanges { get; set; } = true;

        [Description("Messages sent to the player. {status}, {seconds} and {max} are placeholders.")]
        public MessagesConfig Messages { get; set; } = new MessagesConfig();
    }

    public class MessagesConfig
    {
        public string Set { get; set; } = "Durumun ayarlandı: {status}";

        public string Cleared { get; set; } = "Durumun kaldırıldı.";

        public string NothingToClear { get; set; } = "Zaten bir durumun yok.";

        public string Current { get; set; } = "Şu anki durumun: {status}";

        public string Usage { get; set; } = "Kullanım: .status <metin>   |   Kaldırmak için: .status sil";

        public string Cooldown { get; set; } = "Biraz bekle, {seconds} sn sonra tekrar değiştirebilirsin.";

        public string NotAlive { get; set; } = "Durum ayarlamak için yaşayan bir karakterin olmalı.";

        public string Empty { get; set; } = "Durum metni boş olamaz.";

        public string TooLong { get; set; } = "Durum en fazla {max} karakter olabilir.";
    }
}
