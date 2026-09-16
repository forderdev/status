## Garry's Mod oyununda bulunan /status komutunu SCP:SL'ye getirsek nasıl olurdu?

#### İşte böyle.

!\[](https://i.imgur.com/rKROWnv.gif)

Oyuncunun, karakterinde bulunan sürekli bir durumu göstermesi için yapılan bir plugindir.

SCP:SL 14.2.7, LabAPI 1.1.7. Sadece sunucu tarafı, client modu gerekmez.

### Kurulum

1. dll\\PlayerStatus.dll dosyasını ...\\LabAPI\\plugins<port>\\ klasörüne at.
2. Sunucuyu başlat. Configin oluşma klasörü: ...\\LabAPI\\configs<port>\\PlayerStatus\\config.yml

### Kullanım (Komutlar):

* `.status <metin>` ya da `.durum <metin>` — durumunu değiştirir.
* `.status sil` — durumunu siler. (`kaldır`, `temizle`, `clear` vs. de olur.)
* `.status` — mevcut durumunu gösterir.

### Admin Komutları:

* `playerstatus list`
* `playerstatus clear <oyuncular|all>`
* `playerstatus set <oyuncu> <metin>`

### Config

|Ayar|Varsayılan|Ne olduğu|
|-|-|-|
|`max\_length`|72|karakter sınırı|
|`max\_chars\_per\_line`|28|satır uzunluğu|
|`max\_lines`|2|satır sayısı|
|`min\_text\_size`|60|en fazla küçülme (%)|
|`scale`|0.12|yazı boyutu|
|`height\_above\_head`|0.3|gözün kaç metre üstünde|
|`max\_view\_distance`|12|görünme mesafesi (m)|
|`cooldown`|2|saniye|
|`show\_to\_self`|false|kendi durumunu görme|
|`mode`|Billboard|`Front`: yazı sabit, oyuncunun önüne bakar|

Varsayılan configlere geri dönmek istiyorsan config dosyasını silmeniz gereklidir, sildikten sonra otomatik olarak varsayılanlar gelecektir.

(Tablo Yapay Zeka tarafından oluşturulmuştur.)

### Build

```
dotnet build source\\PlayerStatus.csproj -c Release
```

lib\\ klasörü repoda bulunmuyor. Plugini derlemek için sunucunun SCPSL\_Data\\Managed klasöründen şu dll'leri lib\\ içine kopyala:

```
Assembly-CSharp.dll
Assembly-CSharp-firstpass.dll
CommandSystem.Core.dll
LabApi.dll
Mirror.dll
NorthwoodLib.dll
Pooling.dll
YamlDotNet.dll
UnityEngine.dll
UnityEngine.CoreModule.dll
UnityEngine.PhysicsModule.dll
```

\---

## What if we brought Garry's Mod's /status command to SCP:SL?

#### Like this.

!\[](https://i.imgur.com/rKROWnv.gif)

This plugin allows players to display a persistent status message on their character.

SCP:SL 14.2.7, LabAPI 1.1.7. Server-side only, no client mod required.

### Installation

1. Drop `dll\\PlayerStatus.dll` into `%AppData%\\SCP Secret Laboratory\\LabAPI\\plugins\\<port>\\`.
2. Start the server. The config is created at `%AppData%\\SCP Secret Laboratory\\LabAPI\\configs\\<port>\\PlayerStatus\\config.yml`

### Usage (Commands):

* `.status <text>` or `.durum <text>` — Changes your status.
* `.status sil` — Removes your status. (`remove`, `clear`, etc. also work.)
* `.status` — Shows your current status.

### Admin Commands:

* `playerstatus list`
* `playerstatus clear <players|all>`
* `playerstatus set <player> <text>`

### Config

|Setting|Default|Description|
|-|-|-|
|`max\_length`|72|Maximum character limit|
|`max\_chars\_per\_line`|28|Maximum characters per line|
|`max\_lines`|2|Maximum number of lines|
|`min\_text\_size`|60|Maximum text shrinking (%)|
|`scale`|0.12|Text size|
|`height\_above\_head`|0.3|How many meters above eye level the text appears|
|`max\_view\_distance`|12|Maximum visibility distance (m)|
|`cooldown`|2|Cooldown in seconds|
|`show\_to\_self`|false|Whether players can see their own status|
|`mode`|Billboard|`Front`: The text remains fixed and faces the front of the player|

If you want to restore the default configuration values, simply delete the config file. The default settings will be regenerated automatically afterward.

*(The table and the translation was made by AI.)*

### Build

```
dotnet build source\\PlayerStatus.csproj -c Release
```

`lib\\` is not in the repository. To build, copy these DLLs from your server's `SCPSL\_Data\\Managed` folder into `lib\\`:

```
Assembly-CSharp.dll
Assembly-CSharp-firstpass.dll
CommandSystem.Core.dll
LabApi.dll
Mirror.dll
NorthwoodLib.dll
Pooling.dll
YamlDotNet.dll
UnityEngine.dll
UnityEngine.CoreModule.dll
UnityEngine.PhysicsModule.dll
```

