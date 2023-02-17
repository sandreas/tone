using System.Linq;
using tone.Common.Extensions.String;
using tone.Metadata.Formats;
using Xunit;
using Xunit.Abstractions;

namespace tone.Tests.Metadata.Formats;

public class FfmetadataFormatTest
{
    const string SimpleFfmetadata = @";FFMETADATA1
major_brand=isom
minor_version=512
compatible_brands=isomiso2mp41
title=A title
artist=An Artist
composer=A composer
album=An Album
date=2011
description=A description
comment=A comment
encoder=Lavf56.40.101
[CHAPTER]
TIMEBASE=1/1000
START=0
END=264034
title=001
[CHAPTER]
TIMEBASE=1/1000
START=264034
END=568958
title=002
[CHAPTER]
TIMEBASE=1/1000
START=568958
END=879455
title=003";

    private const string SynchronizedLyricsFfmeta = @"
;FFMETADATA1
lyrics-eng=[00:01.23]line1 of lyrics
\
[00:04.56]line2 of lyrics
\
[00:07.89]line3 of lyrics
\
[01:03.12]3rd last line of lyrics
\
[02:04.34]2nd last line of lyrics
\
[03:05.67]Last line of lyrics
\
";
    
    private const string AudioBookFfmeta = @"
;FFMETADATA1
major_brand=isom
minor_version=512
compatible_brands=isomiso2mp41
artist=Jennifer Estep
title=Spinnentanz
album=Spinnentanz
genre=Fantasy
composer=Tanja Fornaro
copyright=Audible Studios
description=Elemental Assassin 2: Zwar hat sich die erfolgreiche Auftragsmörderin Gin Blanco offiziell zur Ruhe gesetzt, doch der Ärger reißt einfach nicht ab:\
\
Als zwei Punks versuchen, Gins Restaurant auszurauben, fallen Schüsse - doch ungewöhnlicherweise is...
date=2016-08-17T22:00:00
grouping=
sort_name=Elemental Assassin 2 - Spinnentanz
sort_album=Elemental Assassin 2 - Spinnentanz
synopsis=Elemental Assassin 2: Zwar hat sich die erfolgreiche Auftragsmörderin Gin Blanco offiziell zur Ruhe gesetzt, doch der Ärger reißt einfach nicht ab: Als zwei Punks versuchen, Gins Restaurant auszurauben, fallen Schüsse - doch ungewöhnlicherweise ist nicht ""Die Spinne"" ihr Ziel, sondern ihr Protegé Violet Fox. Gin muss herausfinden, wer hinter dem Angriff steckt und warum. Wenn sexy Detective Caine sie nur nicht ständig ablenken würde! Als dann auch noch ein anderer Mann ins Spiel kommt, wird dem Steinelementar Gin trotz ihrer Eismagie richtig heiß...
AUDIBLE_ASIN=B01J1X5Z9C
purchase_date=2017/06/14
media_type=2
gapless_playback=1
PART=2
encoder=Lavf58.29.100
[CHAPTER]
TIMEBASE=1/44100
START=0
END=565759
title=Intro
[CHAPTER]
TIMEBASE=1/44100
START=565759
END=14336778
title=1. Kapitel – »Stehen bleiben! Keine … (1/5)
[CHAPTER]
TIMEBASE=1/44100
START=14336778
END=27941187
title=1. Kapitel – »Stehen bleiben! Keine … (2/5)
[CHAPTER]
TIMEBASE=1/44100
START=27941187
END=41394686
title=1. Kapitel – »Stehen bleiben! Keine … (3/5)
[CHAPTER]
TIMEBASE=1/44100
START=41394686
END=55170953
title=1. Kapitel – »Stehen bleiben! Keine … (4/5)
[CHAPTER]
TIMEBASE=1/44100
START=55170953
END=72859860
title=1. Kapitel – »Stehen bleiben! Keine … (5/5)
[CHAPTER]
TIMEBASE=1/44100
START=72859860
END=86222116
title=2. Kapitel – Zwanzig Minuten später … (1/4)
[CHAPTER]
TIMEBASE=1/44100
START=86222116
END=100519027
title=2. Kapitel – Zwanzig Minuten später … (2/4)
[CHAPTER]
TIMEBASE=1/44100
START=100519027
END=113916431
title=2. Kapitel – Zwanzig Minuten später … (3/4)
[CHAPTER]
TIMEBASE=1/44100
START=113916431
END=138314139
title=2. Kapitel – Zwanzig Minuten später … (4/4)
[CHAPTER]
TIMEBASE=1/44100
START=138314139
END=151767726
title=3. Kapitel – Es verging kaum … (1/5)
[CHAPTER]
TIMEBASE=1/44100
START=151767726
END=165475285
title=3. Kapitel – Es verging kaum … (2/5)
[CHAPTER]
TIMEBASE=1/44100
START=165475285
END=178751987
title=3. Kapitel – Es verging kaum … (3/5)
[CHAPTER]
TIMEBASE=1/44100
START=178751987
END=193809976
title=3. Kapitel – Es verging kaum … (4/5)
[CHAPTER]
TIMEBASE=1/44100
START=193809976
END=216940735
title=3. Kapitel – Es verging kaum … (5/5)
[CHAPTER]
TIMEBASE=1/44100
START=216940735
END=231617259
title=4. Kapitel – »Ich werde diesen … (1/4)
[CHAPTER]
TIMEBASE=1/44100
START=231617259
END=245537600
title=4. Kapitel – »Ich werde diesen … (2/4)
[CHAPTER]
TIMEBASE=1/44100
START=245537600
END=259312411
title=4. Kapitel – »Ich werde diesen … (3/4)
[CHAPTER]
TIMEBASE=1/44100
START=259312411
END=275007866
title=4. Kapitel – »Ich werde diesen … (4/4)
[CHAPTER]
TIMEBASE=1/44100
START=275007866
END=288426570
title=5. Kapitel – Eine Kugel schlug … (1/3)
[CHAPTER]
TIMEBASE=1/44100
START=288426570
END=301934400
title=5. Kapitel – Eine Kugel schlug … (2/3)
[CHAPTER]
TIMEBASE=1/44100
START=301934400
END=318083732
title=5. Kapitel – Eine Kugel schlug … (3/3)
[CHAPTER]
TIMEBASE=1/44100
START=318083732
END=346604966
title=6. Kapitel – Das Mädchen. Violet. …
[CHAPTER]
TIMEBASE=1/44100
START=346604966
END=360508241
title=7. Kapitel – »Schon was rausgekriegt?« (1/4)
[CHAPTER]
TIMEBASE=1/44100
START=360508241
END=373797952
title=7. Kapitel – »Schon was rausgekriegt?« (2/4)
[CHAPTER]
TIMEBASE=1/44100
START=373797952
END=387576071
title=7. Kapitel – »Schon was rausgekriegt?« (3/4)
[CHAPTER]
TIMEBASE=1/44100
START=387576071
END=407590856
title=7. Kapitel – »Schon was rausgekriegt?« (4/4)
[CHAPTER]
TIMEBASE=1/44100
START=407590856
END=421391642
title=8. Kapitel – Schon als ich … (1/3)
[CHAPTER]
TIMEBASE=1/44100
START=421391642
END=435095188
title=8. Kapitel – Schon als ich … (2/3)
[CHAPTER]
TIMEBASE=1/44100
START=435095188
END=460217900
title=8. Kapitel – Schon als ich … (3/3)
[CHAPTER]
TIMEBASE=1/44100
START=460217900
END=491879583
title=9. Kapitel – Jo-Jo trat zur …
[CHAPTER]
TIMEBASE=1/44100
START=491879583
END=522237758
title=10. Kapitel – Finn und ich …
[CHAPTER]
TIMEBASE=1/44100
START=522237758
END=535540567
title=11. Kapitel – Ich schob mein … (1/3)
[CHAPTER]
TIMEBASE=1/44100
START=535540567
END=549546947
title=11. Kapitel – Ich schob mein … (2/3)
[CHAPTER]
TIMEBASE=1/44100
START=549546947
END=567282423
title=11. Kapitel – Ich schob mein … (3/3)
[CHAPTER]
TIMEBASE=1/44100
START=567282423
END=580715680
title=12. Kapitel – Sobald sie sich … (1/4)
[CHAPTER]
TIMEBASE=1/44100
START=580715680
END=594684576
title=12. Kapitel – Sobald sie sich … (2/4)
[CHAPTER]
TIMEBASE=1/44100
START=594684576
END=608801559
title=12. Kapitel – Sobald sie sich … (3/4)
[CHAPTER]
TIMEBASE=1/44100
START=608801559
END=628782696
title=12. Kapitel – Sobald sie sich … (4/4)
[CHAPTER]
TIMEBASE=1/44100
START=628782696
END=642863120
title=13. Kapitel – »Langsam nervt es«, … (1/4)
[CHAPTER]
TIMEBASE=1/44100
START=642863120
END=656164782
title=13. Kapitel – »Langsam nervt es«, … (2/4)
[CHAPTER]
TIMEBASE=1/44100
START=656164782
END=669797724
title=13. Kapitel – »Langsam nervt es«, … (3/4)
[CHAPTER]
TIMEBASE=1/44100
START=669797724
END=695681249
title=13. Kapitel – »Langsam nervt es«, … (4/4)
[CHAPTER]
TIMEBASE=1/44100
START=695681249
END=709559254
title=14. Kapitel – Sobald Jake McAllisters … (1/3)
[CHAPTER]
TIMEBASE=1/44100
START=709559254
END=722839837
title=14. Kapitel – Sobald Jake McAllisters … (2/3)
[CHAPTER]
TIMEBASE=1/44100
START=722839837
END=742730745
title=14. Kapitel – Sobald Jake McAllisters … (3/3)
[CHAPTER]
TIMEBASE=1/44100
START=742730745
END=756434159
title=15. Kapitel – Jetzt wusste ich, … (1/2)
[CHAPTER]
TIMEBASE=1/44100
START=756434159
END=783458771
title=15. Kapitel – Jetzt wusste ich, … (2/2)
[CHAPTER]
TIMEBASE=1/44100
START=783458771
END=796867332
title=16. Kapitel – Der SUV … (1/4)
[CHAPTER]
TIMEBASE=1/44100
START=796867332
END=811028151
title=16. Kapitel – Der SUV … (2/4)
[CHAPTER]
TIMEBASE=1/44100
START=811028151
END=824372943
title=16. Kapitel – Der SUV … (3/4)
[CHAPTER]
TIMEBASE=1/44100
START=824372943
END=838382455
title=16. Kapitel – Der SUV … (4/4)
[CHAPTER]
TIMEBASE=1/44100
START=838382455
END=851804511
title=17. Kapitel – Es folgten keine … (1/3)
[CHAPTER]
TIMEBASE=1/44100
START=851804511
END=865156977
title=17. Kapitel – Es folgten keine … (2/3)
[CHAPTER]
TIMEBASE=1/44100
START=865156977
END=883579708
title=17. Kapitel – Es folgten keine … (3/3)
[CHAPTER]
TIMEBASE=1/44100
START=883579708
END=897002116
title=18. Kapitel – Finn suchte immer … (1/5)
[CHAPTER]
TIMEBASE=1/44100
START=897002116
END=910824423
title=18. Kapitel – Finn suchte immer … (2/5)
[CHAPTER]
TIMEBASE=1/44100
START=910824423
END=925345936
title=18. Kapitel – Finn suchte immer … (3/5)
[CHAPTER]
TIMEBASE=1/44100
START=925345936
END=938675955
title=18. Kapitel – Finn suchte immer … (4/5)
[CHAPTER]
TIMEBASE=1/44100
START=938675955
END=953803225
title=18. Kapitel – Finn suchte immer … (5/5)
[CHAPTER]
TIMEBASE=1/44100
START=953803225
END=981263237
title=19. Kapitel – Für einen Moment …
[CHAPTER]
TIMEBASE=1/44100
START=981263237
END=1013494383
title=20. Kapitel – Selbst mit unserem …
[CHAPTER]
TIMEBASE=1/44100
START=1013494383
END=1026860917
title=21. Kapitel – Nach dem Sex … (1/3)
[CHAPTER]
TIMEBASE=1/44100
START=1026860917
END=1040321825
title=21. Kapitel – Nach dem Sex … (2/3)
[CHAPTER]
TIMEBASE=1/44100
START=1040321825
END=1065142011
title=21. Kapitel – Nach dem Sex … (3/3)
[CHAPTER]
TIMEBASE=1/44100
START=1065142011
END=1079070202
title=22. Kapitel – »Als Erstes müssen … (1/3)
[CHAPTER]
TIMEBASE=1/44100
START=1079070202
END=1093272783
title=22. Kapitel – »Als Erstes müssen … (2/3)
[CHAPTER]
TIMEBASE=1/44100
START=1093272783
END=1108787207
title=22. Kapitel – »Als Erstes müssen … (3/3)
[CHAPTER]
TIMEBASE=1/44100
START=1108787207
END=1122967915
title=23. Kapitel – Am nächsten Tag … (1/3)
[CHAPTER]
TIMEBASE=1/44100
START=1122967915
END=1136909336
title=23. Kapitel – Am nächsten Tag … (2/3)
[CHAPTER]
TIMEBASE=1/44100
START=1136909336
END=1153817100
title=23. Kapitel – Am nächsten Tag … (3/3)
[CHAPTER]
TIMEBASE=1/44100
START=1153817100
END=1167423979
title=24. Kapitel – Kurz nach eins … (1/4)
[CHAPTER]
TIMEBASE=1/44100
START=1167423979
END=1180745310
title=24. Kapitel – Kurz nach eins … (2/4)
[CHAPTER]
TIMEBASE=1/44100
START=1180745310
END=1194028010
title=24. Kapitel – Kurz nach eins … (3/4)
[CHAPTER]
TIMEBASE=1/44100
START=1194028010
END=1211404248
title=24. Kapitel – Kurz nach eins … (4/4)
[CHAPTER]
TIMEBASE=1/44100
START=1211404248
END=1225134739
title=25. Kapitel – Um acht Uhr … (1/3)
[CHAPTER]
TIMEBASE=1/44100
START=1225134739
END=1239389579
title=25. Kapitel – Um acht Uhr … (2/3)
[CHAPTER]
TIMEBASE=1/44100
START=1239389579
END=1256858780
title=25. Kapitel – Um acht Uhr … (3/3)
[CHAPTER]
TIMEBASE=1/44100
START=1256858780
END=1273052080
title=26. Kapitel – »Scheiße.« (1/3)
[CHAPTER]
TIMEBASE=1/44100
START=1273052080
END=1286410323
title=26. Kapitel – »Scheiße.« (2/3)
[CHAPTER]
TIMEBASE=1/44100
START=1286410323
END=1309588798
title=26. Kapitel – »Scheiße.« (3/3)
[CHAPTER]
TIMEBASE=1/44100
START=1309588798
END=1323028758
title=27. Kapitel – Für einen Moment … (1/5)
[CHAPTER]
TIMEBASE=1/44100
START=1323028758
END=1337629077
title=27. Kapitel – Für einen Moment … (2/5)
[CHAPTER]
TIMEBASE=1/44100
START=1337629077
END=1351055278
title=27. Kapitel – Für einen Moment … (3/5)
[CHAPTER]
TIMEBASE=1/44100
START=1351055278
END=1364792208
title=27. Kapitel – Für einen Moment … (4/5)
[CHAPTER]
TIMEBASE=1/44100
START=1364792208
END=1390085278
title=27. Kapitel – Für einen Moment … (5/5)
[CHAPTER]
TIMEBASE=1/44100
START=1390085278
END=1424639392
title=28. Kapitel – Owen Grayson führte …
[CHAPTER]
TIMEBASE=1/44100
START=1424639392
END=1438304880
title=29. Kapitel – »Bist du dir … (1/5)
[CHAPTER]
TIMEBASE=1/44100
START=1438304880
END=1452000708
title=29. Kapitel – »Bist du dir … (2/5)
[CHAPTER]
TIMEBASE=1/44100
START=1452000708
END=1466219606
title=29. Kapitel – »Bist du dir … (3/5)
[CHAPTER]
TIMEBASE=1/44100
START=1466219606
END=1479748251
title=29. Kapitel – »Bist du dir … (4/5)
[CHAPTER]
TIMEBASE=1/44100
START=1479748251
END=1495018582
title=29. Kapitel – »Bist du dir … (5/5)
[CHAPTER]
TIMEBASE=1/44100
START=1495018582
END=1530123329
title=30. Kapitel – »Ein Duell?«, fragte …
[CHAPTER]
TIMEBASE=1/44100
START=1530123329
END=1543838473
title=31. Kapitel – Ich kauerte … (1/5)
[CHAPTER]
TIMEBASE=1/44100
START=1543838473
END=1557311508
title=31. Kapitel – Ich kauerte … (2/5)
[CHAPTER]
TIMEBASE=1/44100
START=1557311508
END=1571044689
title=31. Kapitel – Ich kauerte … (3/5)
[CHAPTER]
TIMEBASE=1/44100
START=1571044689
END=1584626519
title=31. Kapitel – Ich kauerte … (4/5)
[CHAPTER]
TIMEBASE=1/44100
START=1584626519
END=1605641889
title=31. Kapitel – Ich kauerte … (5/5)
[CHAPTER]
TIMEBASE=1/44100
START=1605641889
END=1619727826
title=32. Kapitel – Ich krabbelte auf … (1/3)
[CHAPTER]
TIMEBASE=1/44100
START=1619727826
END=1633302335
title=32. Kapitel – Ich krabbelte auf … (2/3)
[CHAPTER]
TIMEBASE=1/44100
START=1633302335
END=1649288365
title=32. Kapitel – Ich krabbelte auf … (3/3)
[CHAPTER]
TIMEBASE=1/44100
START=1649288365
END=1662657809
title=33. Kapitel – Es folgten eine … (1/4)
[CHAPTER]
TIMEBASE=1/44100
START=1662657809
END=1676323252
title=33. Kapitel – Es folgten eine … (2/4)
[CHAPTER]
TIMEBASE=1/44100
START=1676323252
END=1689761536
title=33. Kapitel – Es folgten eine … (3/4)
[CHAPTER]
TIMEBASE=1/44100
START=1689761536
END=1708480310
title=33. Kapitel – Es folgten eine … (4/4)
[CHAPTER]
TIMEBASE=1/44100
START=1708480310
END=1721959387
title=34. Kapitel – Das Unglück in … (1/4)
[CHAPTER]
TIMEBASE=1/44100
START=1721959387
END=1735396789
title=34. Kapitel – Das Unglück in … (2/4)
[CHAPTER]
TIMEBASE=1/44100
START=1735396789
END=1748764028
title=34. Kapitel – Das Unglück in … (3/4)
[CHAPTER]
TIMEBASE=1/44100
START=1748764028
END=1770175195
title=34. Kapitel – Das Unglück in … (4/4)
[CHAPTER]
TIMEBASE=1/44100
START=1770175195
END=1798652109
title=35. Kapitel – Ich servierte Violet …
[CHAPTER]
TIMEBASE=1/44100
START=1798652109
END=1799412300
title=Outro
";

    private const string MusicFfmeta = @"
;FFMETADATA1
title=Crash
artist=12 Stones
track=1/12
album=12 Stones
disc=1/1
date=2002-04-23
genre=Alternative Rock
TBPM=0
compilation=0
TMED=CD
language=eng
TIPL=arranger
album_artist=12 Stones
publisher=Wind‐up
artist-sort=12 Stones
TDOR=2002-04-23
Script=Latn
ASIN=B0000649OY
Artist Credit=12 Stones
ALBUMARTISTSORT=12 Stones
CATALOGNUMBER=60150-13069-2
MusicBrainz Album Type=album
Album Artist Credit=12 Stones
REPLAYGAIN_ALBUM_GAIN=-9.98 dB
REPLAYGAIN_ALBUM_PEAK=1.308612
REPLAYGAIN_TRACK_GAIN=-9.94 dB
REPLAYGAIN_TRACK_PEAK=1.200538
MusicBrainz Album Status=Official
MusicBrainz Album Release Country=US
Acoustid Id=4d8cba28-7286-4500-ad86-aa639c7f0333
MusicBrainz Album Id=5ffc2a4d-241b-4c63-ae33-42444365540f
MusicBrainz Artist Id=6f81a7dc-be31-4498-ae95-6d994ffec614
MusicBrainz Album Artist Id=6f81a7dc-be31-4498-ae95-6d994ffec614
MusicBrainz Release Group Id=c69ae98f-a2f8-3515-8ca6-a3644ba31905
MusicBrainz Release Track Id=3a34ad6c-3678-3a3a-afc1-324b526ccc08
iTunNORM= 00002687 00002687 00006051 00006051 00000000 00000000 000099AB 000099AB 00000000 00000000
lyrics-XXX=As I lie here tossing in my bed\
Lost in my fears remembering what you said\
And I try to hide the truth within\
The mask of myself shows its face again\
Still I lie here time and time again\
Will you deny me when we meet again?\
\
And I feel like I'm falling\
Farther every day\
But I know that you're there\
Watching over me\
And I feel like I'm drowning\
The waves crashing over me\
But I know that your love\
It will set me free\
\
As I find truth where I found it times before\
As I search for your hope\
I'm finding so much more\
And I try to be more like you\
And I deny myself to prove my heart is true\
\
And I feel like I'm falling\
Farther every day\
But I know that you're there\
Watching over me\
And I feel like I'm drowning\
The waves crashing over me\
But I know that your love\
It will set me free\
\
I hear your voice calling\
The time has come for me\
Inside this life I'm living\
There's nothing left for me\
My mind is slowly fading\
So far away from me\
Each time I start crawling\
You're there watching me\
\
And I feel like I'm falling\
Farther every day\
But I know that you're there\
Watching over me\
And I feel like I'm drowning\
The waves crashing over me\
But I know that your love\
It will set me free
Acoustid Fingerprint=AQADtE8URVWEP_hxK5hiDT2Ox7hvfA9ySkSSS12INxx6wXGCfyH6B9fxoE9kNE8w5eg9JcKpHImXI1dmosmH8uiPhg8eHc_B8GjiLHlwHJ-y4xHO5BmSHyHFJC2c9riuRKBWKsRDuDLxXMGPHof6B2sVVdh39M3RfDiO48fx5cZX5NGO5ELzUOiFH13kBzePXkefw0_Ry-jRPCr-fUgcygjVvqAeHhf8sDNKHmeOWoHTw8cJPz06bhdeych1JO-e4cP1oT7sBsyPW7gSXMTD482FkNoL-oZeIrzQo8kf4ol2YY7OwDh-I7-hfjRC5QzhpQ_c4-TxH9c4uA6eo_JxPCLyQ7nYIE_CHT1uw05YBZXkcyAT9HAl6-g7nFqC58h1JA-mTMpR7Y_QjNlRZbqJp_Bj_CR-LfhHPAtyY7_RRFehbdKF2Dsu5UXt4wcbzsHT4RemC5dOVGGInydymRCP8BP2HX_R50FT3TiaBwyOI_wPnYdTF6F1PM-OW7hSPUUf-Arc9JgsjTHuID8SNM8TXPpwPSqaqMev4s6J5p2G0Mp1JP_wr3imTTnOxKhSTImCg7Hg48rhXA-cxHiO_Uhe5CzRD_8bPFUSKriDUjSJD5eH23CP_3iOnjwwHLlGaL6CHA6VG0-UDxfz4dJx-HiEPlKFI0GOqz6e5KiY4zH8o2ek45fwPPiRUzoY5UVzWvgFXTR-eMjBP6jUpUYjOcco4Ufzo2eOXEiO8kZ97Lh2pfiC96hvHN9BHeEhH_GX4z_4qziPMuvhT0fNozrh5EcvGk8lxEdyHWwyHH13uEJ1-njOCz-O6wiZH6KP9A8qT8yCJz3xSMOFHh-cZ0eV5APbB_3R9BeS58iHbmIY-M_gPcNx5bg2wxd-PA8O5sPxH2EcHskz5th2wTm6ZsfYJ8QxlfC6o8eRH7qSI3aPnjVeHpMc4heDNmrQrMf14zx-_MQjPcQ3KUZ46JIu5DkO5xycpPgD8wErHXlziOeF0AqqRE7wnYN97IK_42Q8jOkuOMcP5sLRXKvQZxmu49DUHP6C37jb4iouvcf2wRfRx8WbIx8uJbjh7PiUnZgkdoR4hFfQ_2BSZfjaEA2fpqhfVM6ChhGF46fwah3CXMcLjzpC68hy-B-kvEKOHzeuXAKnLOuCL8xmwcmUA32Rdod25Qi9GU9i9DpwKpzwqEejo9eLJ1fwVQ4-HpxzNJNIoW-MaxWeiYcfJ4gz5dHwBvqOeksGXzIuD84hKSkplMfDB_yQ50irhYa-D_lGBvuGPYuN--gi0XCqDHumE_7iGFP2T8ihM0d2ET_6C4fPjMgiKhF0_Ad1DRwHPToRZs9hVmlQnsMPHr0eOJKYotNynBqSvgxi8hue5EUX5kHz41eEF6G3FVJytgj5wz9R67iO5mHQsziJC5VxohMVPLrxixtq1cFvoNSO_EdFFeLT4keeo-BLPJUVvB-aUEefo9_hR0GoQ88RyilBlU8o-BJqZUeOus7h_UgZNcV1WJGO6ujhih_xBOcM30b6w42PitKhJkwu6HmE5lJmoU9kRDqcHKeIZlGJXJeO52i-GJqpIH9kzOtD9EYzjcOP7tpx_EMZzhn-y_iRvEd-XDf6PMFJbQOlZ0TlCHkmlCfEig4eaJnMI04TyWgyJx1KHtc17NrRjETlLDuiQ95Z_MJRi4IePIePGheXPLgiHs0oD3124kGjEuEXXLnRTI7Qk8fxcMPF4_uRnEcY-nA-dBM-Jbif4K_QICwJXUeOS1sY4ayj4DyaMTuYM8cV41JyPCHRfGD-DG-MPzpOZmgDSryEalxSoVyyIz-hO-GRM9HhJUJ_vMcn4zTR2RJi0gz0HCGX4z6aL0d_4UeuoMx4iGPxiBvcyQgv48QDZn5g7V3h7uiIo890NFeKL0cexQlyfXiUo3IOc2woRI9xGsm4ZBeuLMIrNA_6Jfjxa5isLEeTE4_1IG4qC8mXozx-4jmJw5eCPseVg1nUhMhn6Mido9RmoeGVBKH6Qo_C40QYPhomkvHRVJLRo8_SHGGsJUeZE2LkE10PTY-MnchzOFwe0MeDNCkOPbzgUMfnHW_gZYuMSnsK_jgVVD7iXHDVon_xtDia8ZguxPwhh2jOo3I4FbuwrziXqMGzHCc_9C58iM-O_oowqcmRKrrx8TiZR-iJnEcpRxzM4ZsIXwh_vIoy4skTXAiP5II9HfaPP8ET0oR33HTwBxWFM8h8oRYDPxG0Hj_yRR3a5WhiZkqEr0ZvHVQc9PgPXCVC-4Pz5dDW40eeh8SZHPxC4Rp4ZeKRH9Jv5PyCMssJhzm66Rc4B3mKP4H-aWiY4zn85UjOYM_xHN-q4UpEKUGaPFCUDrn0DE8kyQK1Fz-q4yGinXjGo88PVYwSB2HVB55--MPxYvzRaHiTRcN59AjzQk-Ri0clJQVpfvgPL7zQs1Hw8Mh1PMmgF7EXSbiWg99x6sgXHdpnI8yDHyce3cinQzuD5kd-MOkRysJTaI0W-Iwn2MrRH18OncqG_vAj7MyhLeQRj3owS9i3o3CHUPmDJJTOBedRKlaE5jjOCTfBJT9CH6J7hE-K83jzoF12OIkoCj2FO7FQEX2i48cvPNSDPE9QdRaYTMoMfcmHkGfg_-iLx7gW4_BzjF9x_fg3DY8uoimHD96P98h3-Ax0fkP05Oh5PBLO40qaTApxJmAiJqgo6XgCgRgYBAWABEICIAYMQYQhYagACDngNIAIGiUQUkJRIpAByBAlEBBuGAEBEwjMKkwyhACKjEMECGCEQEYYAAQBBAHJgCUNGICUAMAASBSgjAEHjAHAIGEAAc4BZYQwkgAEkAJEUKqJMEYhQQQAjhhrEACKIMQMAwggAYAQhgFDJIWIIAMAMVAJpIgwBgWkCAEIGIEEBYIJQ60BiBAjjICAASGQIYIARBAEgBAmmGEMAAK0IcoAYAAUBCEikWBAMKgUA0gBILhkgAhCBWLICGYAoMgIkwhyiAhBEBXEEMKBMdQBIRZwRABDGAFCKSMAABIIEAAAgACtGGHAEUIAIMgxAJgRRggjGCEMGUUMoIAxQxIQAgEjhgJRQMiIAEoIwogShDGimAEKcCCYoYoCIBAAAAAnhCDAMGFFEESZJKgQBgAiALJAEsYE0FQYQYDkVAgDAEMEIIGZpQgQIggg0ghAABRAESGJBIgBJIAwgiFEBGXCAOMMwAAIAEQiCClgQFBDAECIEEYSAAhgxBgjEQCIIWKQMBAAa5BTRhBhGCIEMcGkIsQQYIwDzBABCBLCKCOoEgohKIAgQAFhJhAEOAkUEQxIoCQABACnkHEICUAAEU4BRAwwxCMgqVLWAiCEMJQAJxBlSBEkiAQCIGAEgxoI4IwAoBBEBKAAOIKYQkQIIYwyDACIkBFMMIwMcAIoAxASjDDBDBAYOASQNMIRJ4gRQEDIhDIICAkAssYhI4BZgAAgEFIAQWEYqIgYBQQjCBlFjKIEKACccsQIAgAERgDFgCFAKUeMIEA4oCRAQhLigCAEASCBUUQYxgBRBjADjACEEQYIQsAIIKAgkgIEtBMAIAOII4ggQoiQkgEggDBCKCEaIAARqpBRBhhGBCNGKCSgIIAwQKRzxCBBBTJIGQEMUUQRhYBhghBEADCOMAeAMEhhooBgACDqkBGEOAQUIYAaBZIDAgggGEFEKWQMQMAIZRghAEMK
encoder=Lavf58.29.100
";
    
    
    private readonly FfmetadataFormat _subject;
    private readonly ITestOutputHelper _output;

    public FfmetadataFormatTest(ITestOutputHelper output)
    {
        _subject = new FfmetadataFormat();
        _output = output;
    }

    [Fact]
    public async void TestReadSimple()
    {
        await using var stream = SimpleFfmetadata.StringToStream();
        var actual = await _subject.ReadAsync(stream);
        Assert.True(actual);
        Assert.Equal("A title", actual.Value.Title);
    }

    [Fact]
    public async void TestReadAudioBook()
    {
        await using var stream = AudioBookFfmeta.StringToStream();
        var actual = await _subject.ReadAsync(stream);
        Assert.True(actual);
        Assert.Equal("Spinnentanz", actual.Value.Title);
        Assert.Equal(@"Elemental Assassin 2: Zwar hat sich die erfolgreiche Auftragsmörderin Gin Blanco offiziell zur Ruhe gesetzt, doch der Ärger reißt einfach nicht ab:

Als zwei Punks versuchen, Gins Restaurant auszurauben, fallen Schüsse - doch ungewöhnlicherweise is...", actual.Value.Description);
        Assert.Equal(111, actual.Value.Chapters.Count);

        var firstChapter = actual.Value.Chapters.First();
        Assert.Equal(0u, firstChapter.StartTime);
        Assert.Equal(12829u, firstChapter.EndTime);

        var ninthChapter = actual.Value.Chapters.ElementAt(8);
        Assert.Equal(2279343u, ninthChapter.StartTime);
        Assert.Equal(2583139u, ninthChapter.EndTime);
        
        var lastChapter = actual.Value.Chapters.Last();
        Assert.Equal(40785762u, lastChapter.StartTime);
        Assert.Equal(40803000u, lastChapter.EndTime);
    }
}