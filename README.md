## Garry's Mod oyununda bulunan /status komutunu SCP:SL'ye getirsek nasıl olurdu?

#### İşte böyle.

!\[](https://i.imgur.com/rKROWnv.gif)

Oyuncunun, karakterinde bulunan sürekli bir durumu göstermesi için yapılan bir plugindir.

SCP:SL 14.2.7, LabAPI 1.1.7

### Kurulum

1. PlayerStatus.dll dosyasını `...\LabAPI\plugins\<port>\` klasörüne at.
2. Sunucuyu başlat. Config'in oluşma klasörü: `...\LabAPI\configs\<port>\PlayerStatus\config.yml`

### Kullanım (Komutlar):

.status <text> ya da .durum <text> ; durumunu değiştirir.

.status sil ; (kaldır, temizle, clear vs. de olur.) durumunu siler.

.status ; Mevcut durumunu gösterir

### Admin Komutları:

playerstatus list

playerstatus clear <oyuncular|all>

playerstatus set <oyuncu> <metin>

## Config

|Ayar|Varsayılan|Ne olduğu|
|-|-|-|
|`max_length`|72|karakter sınırı|
|`max_chars_per_line`|28|satır uzunluğu|
|`max_lines`|2|satır sayısı|
|`min_text_size`|60|en fazla küçülme (%)|
|`scale`|0.12|yazı boyutu|
|`height_above_head`|0.3|gözün kaç metre üstünde|
|`max_view_distance`|12|görünme mesafesi (m)|
|`cooldown`|2|saniye|
|`show_to_self`|false|kendi durumunu görme|
|`mode`|Billboard|`Front`: yazı sabit, oyuncunun önüne bakar|

Varsayılan configlere geri dönmek istiyorsan config dosyasını silmeniz gereklidir, sildikten sonra otomatik olarak varsayılanlar gelecektir.

(Tablo Yapay Zeka tarafından oluşturulmuştur.)

### Build

```
dotnet build source\PlayerStatus.csproj -c Release
```

lib\ klasörünü repoya yükleyemiyorum maalesef. Plugini derlemek için sunucunun SCPSL_Data\Managed klasöründen aşağıdaki dll'leri lib\ içine kopyalaman gerekli.

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

---

## What if we brought Garry's Mod's /status command to SCP:SL?

#### Like this.

!\[](https://i.imgur.com/rKROWnv.gif)

This plugin allows players to display a persistent status message on their character.

### Installation

1. Drop `dll\PlayerStatus.dll` into `%AppData%\SCP Secret Laboratory\LabAPI\plugins\<port>\`.
2. Start the server. The config is created at `%AppData%\SCP Secret Laboratory\LabAPI\configs\<port>\PlayerStatus\config.yml`

### Usage (Commands):

`.status <text>` or `.durum <text>` — Changes your status.

`.status sil` — Removes your status. (`remove`, `clear`, etc. also work.)

`.status` — Shows your current status.

### Admin Commands:

`playerstatus list`

`playerstatus clear <players|all>`

`playerstatus set <player> <text>`

## Config

|Setting|Default|Description|
|-|-|-|
|`max_length`|72|Maximum character limit|
|`max_chars_per_line`|28|Maximum characters per line|
|`max_lines`|2|Maximum number of lines|
|`min_text_size`|60|Maximum text shrinking (%)|
|`scale`|0.12|Text size|
|`height_above_head`|0.3|How many meters above eye level the text appears|
|`max_view_distance`|12|Maximum visibility distance (m)|
|`cooldown`|2|Cooldown in seconds|
|`show_to_self`|false|Whether players can see their own status|
|`mode`|Billboard|`Front`: The text remains fixed and faces the front of the player|

If you want to restore the default configuration values, simply delete the config file. The default settings will be regenerated automatically afterward.

### Build

```
dotnet build source\PlayerStatus.csproj -c Release
```

`lib\` is not in the repository. To build, copy these DLLs from your server's `SCPSL_Data\Managed` folder into `lib\`:

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
*(The table and the translation was made by AI.)*
