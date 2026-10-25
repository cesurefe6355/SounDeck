<div align="center">

<!DOCTYPE html>
<html>
<head>
<style>
@keyframes waveform {
  0%   { transform: scaleY(0.3); }
  50%  { transform: scaleY(1.0); }
  100% { transform: scaleY(0.3); }
}
</style>
</head>
</html>

<br>

<img src="https://capsule-render.vercel.app/api?type=waving&color=7C5CFF&height=200&section=header&text=SoundDeck&fontSize=72&fontColor=ffffff&fontAlignY=38&desc=Ses%20Panosu%20%26%20Yayın%20Mikseri&descAlignY=58&descSize=22&animation=fadeIn" width="100%"/>

<br>

<!-- Live Waveform SVG Animation - Tamamen özel, kimsenin yazmadığı bir animasyon -->
<svg xmlns="http://www.w3.org/2000/svg" width="700" height="80" viewBox="0 0 700 80">
  <defs>
    <linearGradient id="waveGrad" x1="0" y1="0" x2="1" y2="0">
      <stop offset="0%"   stop-color="#7C5CFF" stop-opacity="0.2"/>
      <stop offset="30%"  stop-color="#7C5CFF"/>
      <stop offset="70%"  stop-color="#8F73FF"/>
      <stop offset="100%" stop-color="#8F73FF" stop-opacity="0.2"/>
    </linearGradient>
    <filter id="glow">
      <feGaussianBlur stdDeviation="2.5" result="coloredBlur"/>
      <feMerge><feMergeNode in="coloredBlur"/><feMergeNode in="SourceGraphic"/></feMerge>
    </filter>
  </defs>

  <!-- Bar 1 -->
  <rect x="2"   y="30" width="5" height="20" rx="2.5" fill="url(#waveGrad)" filter="url(#glow)">
    <animate attributeName="height" values="8;28;12;35;8"  dur="1.8s" repeatCount="indefinite"/>
    <animate attributeName="y"      values="36;26;34;22;36" dur="1.8s" repeatCount="indefinite"/>
  </rect>
  <!-- Bar 2 -->
  <rect x="11"  y="20" width="5" height="40" rx="2.5" fill="url(#waveGrad)" filter="url(#glow)">
    <animate attributeName="height" values="40;15;55;20;40" dur="2.1s" repeatCount="indefinite"/>
    <animate attributeName="y"      values="20;32;12;30;20" dur="2.1s" repeatCount="indefinite"/>
  </rect>
  <!-- Bar 3 -->
  <rect x="20"  y="10" width="5" height="60" rx="2.5" fill="url(#waveGrad)" filter="url(#glow)">
    <animate attributeName="height" values="60;25;70;30;60" dur="1.5s" repeatCount="indefinite"/>
    <animate attributeName="y"      values="10;27;5;25;10"  dur="1.5s" repeatCount="indefinite"/>
  </rect>
  <!-- Bar 4 -->
  <rect x="29"  y="15" width="5" height="50" rx="2.5" fill="url(#waveGrad)" filter="url(#glow)">
    <animate attributeName="height" values="50;70;20;65;50" dur="2.4s" repeatCount="indefinite"/>
    <animate attributeName="y"      values="15;5;30;7;15"   dur="2.4s" repeatCount="indefinite"/>
  </rect>
  <!-- Bar 5 -->
  <rect x="38"  y="5"  width="5" height="70" rx="2.5" fill="url(#waveGrad)" filter="url(#glow)">
    <animate attributeName="height" values="70;40;75;35;70" dur="1.9s" repeatCount="indefinite"/>
    <animate attributeName="y"      values="5;20;2;22;5"    dur="1.9s" repeatCount="indefinite"/>
  </rect>
  <!-- Bar 6 -->
  <rect x="47"  y="8"  width="5" height="64" rx="2.5" fill="url(#waveGrad)" filter="url(#glow)">
    <animate attributeName="height" values="64;30;72;28;64" dur="2.2s" repeatCount="indefinite"/>
    <animate attributeName="y"      values="8;25;4;26;8"    dur="2.2s" repeatCount="indefinite"/>
  </rect>
  <!-- Bar 7 -->
  <rect x="56"  y="18" width="5" height="44" rx="2.5" fill="url(#waveGrad)" filter="url(#glow)">
    <animate attributeName="height" values="44;68;22;72;44" dur="1.6s" repeatCount="indefinite"/>
    <animate attributeName="y"      values="18;6;29;4;18"   dur="1.6s" repeatCount="indefinite"/>
  </rect>
  <!-- Bar 8 -->
  <rect x="65"  y="25" width="5" height="30" rx="2.5" fill="url(#waveGrad)" filter="url(#glow)">
    <animate attributeName="height" values="30;55;18;60;30" dur="2.0s" repeatCount="indefinite"/>
    <animate attributeName="y"      values="25;12;31;10;25" dur="2.0s" repeatCount="indefinite"/>
  </rect>
  <!-- Bar 9 -->
  <rect x="74"  y="12" width="5" height="56" rx="2.5" fill="url(#waveGrad)" filter="url(#glow)">
    <animate attributeName="height" values="56;20;65;18;56" dur="2.3s" repeatCount="indefinite"/>
    <animate attributeName="y"      values="12;30;7;31;12"  dur="2.3s" repeatCount="indefinite"/>
  </rect>
  <!-- Bar 10 -->
  <rect x="83"  y="4"  width="5" height="72" rx="2.5" fill="url(#waveGrad)" filter="url(#glow)">
    <animate attributeName="height" values="72;45;76;40;72" dur="1.7s" repeatCount="indefinite"/>
    <animate attributeName="y"      values="4;17;2;20;4"    dur="1.7s" repeatCount="indefinite"/>
  </rect>

  <!-- Middle gap (playhead center) -->
  <line x1="350" y1="0" x2="350" y2="80" stroke="#7C5CFF" stroke-width="1.5" stroke-dasharray="4,3" opacity="0.6">
    <animate attributeName="opacity" values="0.6;1;0.6" dur="1.2s" repeatCount="indefinite"/>
  </line>

  <!-- Right side bars mirrored (600..700) -->
  <rect x="612" y="30" width="5" height="20" rx="2.5" fill="url(#waveGrad)" filter="url(#glow)">
    <animate attributeName="height" values="8;28;12;35;8"  dur="1.8s" repeatCount="indefinite"/>
    <animate attributeName="y"      values="36;26;34;22;36" dur="1.8s" repeatCount="indefinite"/>
  </rect>
  <rect x="621" y="20" width="5" height="40" rx="2.5" fill="url(#waveGrad)" filter="url(#glow)">
    <animate attributeName="height" values="40;15;55;20;40" dur="2.1s" repeatCount="indefinite"/>
    <animate attributeName="y"      values="20;32;12;30;20" dur="2.1s" repeatCount="indefinite"/>
  </rect>
  <rect x="630" y="10" width="5" height="60" rx="2.5" fill="url(#waveGrad)" filter="url(#glow)">
    <animate attributeName="height" values="60;25;70;30;60" dur="1.5s" repeatCount="indefinite"/>
    <animate attributeName="y"      values="10;27;5;25;10"  dur="1.5s" repeatCount="indefinite"/>
  </rect>
  <rect x="639" y="15" width="5" height="50" rx="2.5" fill="url(#waveGrad)" filter="url(#glow)">
    <animate attributeName="height" values="50;70;20;65;50" dur="2.4s" repeatCount="indefinite"/>
    <animate attributeName="y"      values="15;5;30;7;15"   dur="2.4s" repeatCount="indefinite"/>
  </rect>
  <rect x="648" y="5"  width="5" height="70" rx="2.5" fill="url(#waveGrad)" filter="url(#glow)">
    <animate attributeName="height" values="70;40;75;35;70" dur="1.9s" repeatCount="indefinite"/>
    <animate attributeName="y"      values="5;20;2;22;5"    dur="1.9s" repeatCount="indefinite"/>
  </rect>
  <rect x="657" y="8"  width="5" height="64" rx="2.5" fill="url(#waveGrad)" filter="url(#glow)">
    <animate attributeName="height" values="64;30;72;28;64" dur="2.2s" repeatCount="indefinite"/>
    <animate attributeName="y"      values="8;25;4;26;8"    dur="2.2s" repeatCount="indefinite"/>
  </rect>
  <rect x="666" y="18" width="5" height="44" rx="2.5" fill="url(#waveGrad)" filter="url(#glow)">
    <animate attributeName="height" values="44;68;22;72;44" dur="1.6s" repeatCount="indefinite"/>
    <animate attributeName="y"      values="18;6;29;4;18"   dur="1.6s" repeatCount="indefinite"/>
  </rect>
  <rect x="675" y="25" width="5" height="30" rx="2.5" fill="url(#waveGrad)" filter="url(#glow)">
    <animate attributeName="height" values="30;55;18;60;30" dur="2.0s" repeatCount="indefinite"/>
    <animate attributeName="y"      values="25;12;31;10;25" dur="2.0s" repeatCount="indefinite"/>
  </rect>
  <rect x="684" y="12" width="5" height="56" rx="2.5" fill="url(#waveGrad)" filter="url(#glow)">
    <animate attributeName="height" values="56;20;65;18;56" dur="2.3s" repeatCount="indefinite"/>
    <animate attributeName="y"      values="12;30;7;31;12"  dur="2.3s" repeatCount="indefinite"/>
  </rect>
  <rect x="693" y="4"  width="5" height="72" rx="2.5" fill="url(#waveGrad)" filter="url(#glow)">
    <animate attributeName="height" values="72;45;76;40;72" dur="1.7s" repeatCount="indefinite"/>
    <animate attributeName="y"      values="4;17;2;20;4"    dur="1.7s" repeatCount="indefinite"/>
  </rect>

  <!-- Dense center waveform bars (100..600) – 50 bars -->
  <rect x="100" y="20" width="4" height="40" rx="2" fill="url(#waveGrad)" filter="url(#glow)" opacity="0.85">
    <animate attributeName="height" values="40;60;22;70;40" dur="1.55s" repeatCount="indefinite" begin="0s"/>
    <animate attributeName="y"      values="20;10;29;5;20"  dur="1.55s" repeatCount="indefinite" begin="0s"/>
  </rect>
  <rect x="110" y="8"  width="4" height="64" rx="2" fill="url(#waveGrad)" filter="url(#glow)" opacity="0.85">
    <animate attributeName="height" values="64;30;72;25;64" dur="2.05s" repeatCount="indefinite" begin="0.1s"/>
    <animate attributeName="y"      values="8;25;4;27;8"    dur="2.05s" repeatCount="indefinite" begin="0.1s"/>
  </rect>
  <rect x="120" y="3"  width="4" height="74" rx="2" fill="url(#waveGrad)" filter="url(#glow)" opacity="0.85">
    <animate attributeName="height" values="74;50;78;45;74" dur="1.75s" repeatCount="indefinite" begin="0.15s"/>
    <animate attributeName="y"      values="3;15;1;17;3"    dur="1.75s" repeatCount="indefinite" begin="0.15s"/>
  </rect>
  <rect x="130" y="12" width="4" height="56" rx="2" fill="url(#waveGrad)" filter="url(#glow)" opacity="0.85">
    <animate attributeName="height" values="56;75;30;72;56" dur="2.25s" repeatCount="indefinite" begin="0.05s"/>
    <animate attributeName="y"      values="12;2;25;4;12"   dur="2.25s" repeatCount="indefinite" begin="0.05s"/>
  </rect>
  <rect x="140" y="6"  width="4" height="68" rx="2" fill="url(#waveGrad)" filter="url(#glow)" opacity="0.85">
    <animate attributeName="height" values="68;40;76;35;68" dur="1.65s" repeatCount="indefinite" begin="0.2s"/>
    <animate attributeName="y"      values="6;20;2;22;6"    dur="1.65s" repeatCount="indefinite" begin="0.2s"/>
  </rect>
  <rect x="150" y="15" width="4" height="50" rx="2" fill="url(#waveGrad)" filter="url(#glow)" opacity="0.85">
    <animate attributeName="height" values="50;22;66;18;50" dur="2.15s" repeatCount="indefinite" begin="0.08s"/>
    <animate attributeName="y"      values="15;29;7;31;15"  dur="2.15s" repeatCount="indefinite" begin="0.08s"/>
  </rect>
  <rect x="160" y="4"  width="4" height="72" rx="2" fill="url(#waveGrad)" filter="url(#glow)" opacity="0.85">
    <animate attributeName="height" values="72;48;76;42;72" dur="1.85s" repeatCount="indefinite" begin="0.12s"/>
    <animate attributeName="y"      values="4;16;2;19;4"    dur="1.85s" repeatCount="indefinite" begin="0.12s"/>
  </rect>
  <rect x="170" y="10" width="4" height="60" rx="2" fill="url(#waveGrad)" filter="url(#glow)" opacity="0.85">
    <animate attributeName="height" values="60;35;68;30;60" dur="2.35s" repeatCount="indefinite" begin="0.18s"/>
    <animate attributeName="y"      values="10;22;6;25;10"  dur="2.35s" repeatCount="indefinite" begin="0.18s"/>
  </rect>
  <rect x="180" y="2"  width="4" height="76" rx="2" fill="url(#waveGrad)" filter="url(#glow)" opacity="0.85">
    <animate attributeName="height" values="76;52;78;46;76" dur="1.45s" repeatCount="indefinite" begin="0.22s"/>
    <animate attributeName="y"      values="2;14;1;17;2"    dur="1.45s" repeatCount="indefinite" begin="0.22s"/>
  </rect>
  <rect x="190" y="18" width="4" height="44" rx="2" fill="url(#waveGrad)" filter="url(#glow)" opacity="0.85">
    <animate attributeName="height" values="44;70;20;74;44" dur="2.45s" repeatCount="indefinite" begin="0.07s"/>
    <animate attributeName="y"      values="18;5;30;3;18"   dur="2.45s" repeatCount="indefinite" begin="0.07s"/>
  </rect>
  <!-- 200–600 lighter fill bars at 10px intervals -->
  <rect x="200" y="5"  width="4" height="70" rx="2" fill="url(#waveGrad)" opacity="0.7">
    <animate attributeName="height" values="70;42;74;38;70" dur="1.9s" repeatCount="indefinite" begin="0.03s"/>
    <animate attributeName="y"      values="5;19;3;21;5"    dur="1.9s" repeatCount="indefinite" begin="0.03s"/>
  </rect>
  <rect x="210" y="14" width="4" height="52" rx="2" fill="url(#waveGrad)" opacity="0.7">
    <animate attributeName="height" values="52;28;64;24;52" dur="2.1s" repeatCount="indefinite" begin="0.14s"/>
    <animate attributeName="y"      values="14;26;8;28;14"  dur="2.1s" repeatCount="indefinite" begin="0.14s"/>
  </rect>
  <rect x="220" y="3"  width="4" height="74" rx="2" fill="url(#waveGrad)" opacity="0.7">
    <animate attributeName="height" values="74;50;76;46;74" dur="1.7s" repeatCount="indefinite" begin="0.19s"/>
    <animate attributeName="y"      values="3;15;2;17;3"    dur="1.7s" repeatCount="indefinite" begin="0.19s"/>
  </rect>
  <rect x="230" y="9"  width="4" height="62" rx="2" fill="url(#waveGrad)" opacity="0.7">
    <animate attributeName="height" values="62;38;70;32;62" dur="2.3s" repeatCount="indefinite" begin="0.09s"/>
    <animate attributeName="y"      values="9;21;5;24;9"    dur="2.3s" repeatCount="indefinite" begin="0.09s"/>
  </rect>
  <rect x="240" y="1"  width="4" height="78" rx="2" fill="url(#waveGrad)" opacity="0.7">
    <animate attributeName="height" values="78;55;79;50;78" dur="1.6s" repeatCount="indefinite" begin="0.24s"/>
    <animate attributeName="y"      values="1;12;0;15;1"    dur="1.6s" repeatCount="indefinite" begin="0.24s"/>
  </rect>
  <rect x="250" y="16" width="4" height="48" rx="2" fill="url(#waveGrad)" opacity="0.7">
    <animate attributeName="height" values="48;24;60;20;48" dur="2.0s" repeatCount="indefinite" begin="0.06s"/>
    <animate attributeName="y"      values="16;28;10;30;16" dur="2.0s" repeatCount="indefinite" begin="0.06s"/>
  </rect>
  <rect x="260" y="6"  width="4" height="68" rx="2" fill="url(#waveGrad)" opacity="0.7">
    <animate attributeName="height" values="68;44;72;40;68" dur="2.2s" repeatCount="indefinite" begin="0.16s"/>
    <animate attributeName="y"      values="6;18;4;20;6"    dur="2.2s" repeatCount="indefinite" begin="0.16s"/>
  </rect>
  <rect x="270" y="11" width="4" height="58" rx="2" fill="url(#waveGrad)" opacity="0.7">
    <animate attributeName="height" values="58;34;66;28;58" dur="1.8s" repeatCount="indefinite" begin="0.11s"/>
    <animate attributeName="y"      values="11;23;7;26;11"  dur="1.8s" repeatCount="indefinite" begin="0.11s"/>
  </rect>
  <rect x="280" y="2"  width="4" height="76" rx="2" fill="url(#waveGrad)" opacity="0.7">
    <animate attributeName="height" values="76;53;78;48;76" dur="1.5s" repeatCount="indefinite" begin="0.21s"/>
    <animate attributeName="y"      values="2;13;1;16;2"    dur="1.5s" repeatCount="indefinite" begin="0.21s"/>
  </rect>
  <rect x="290" y="7"  width="4" height="66" rx="2" fill="url(#waveGrad)" opacity="0.7">
    <animate attributeName="height" values="66;42;70;36;66" dur="2.4s" repeatCount="indefinite" begin="0.04s"/>
    <animate attributeName="y"      values="7;19;5;22;7"    dur="2.4s" repeatCount="indefinite" begin="0.04s"/>
  </rect>
  <rect x="300" y="4"  width="4" height="72" rx="2" fill="url(#waveGrad)" opacity="0.65">
    <animate attributeName="height" values="72;46;75;40;72" dur="1.95s" repeatCount="indefinite" begin="0.17s"/>
    <animate attributeName="y"      values="4;17;2;20;4"    dur="1.95s" repeatCount="indefinite" begin="0.17s"/>
  </rect>
  <rect x="310" y="13" width="4" height="54" rx="2" fill="url(#waveGrad)" opacity="0.65">
    <animate attributeName="height" values="54;30;62;26;54" dur="2.15s" repeatCount="indefinite" begin="0.13s"/>
    <animate attributeName="y"      values="13;25;9;27;13"  dur="2.15s" repeatCount="indefinite" begin="0.13s"/>
  </rect>
  <rect x="320" y="0"  width="4" height="80" rx="2" fill="url(#waveGrad)" opacity="0.65">
    <animate attributeName="height" values="80;56;80;52;80" dur="1.65s" repeatCount="indefinite" begin="0.23s"/>
    <animate attributeName="y"      values="0;12;0;14;0"    dur="1.65s" repeatCount="indefinite" begin="0.23s"/>
  </rect>
  <rect x="330" y="8"  width="4" height="64" rx="2" fill="url(#waveGrad)" opacity="0.65">
    <animate attributeName="height" values="64;40;68;34;64" dur="2.35s" repeatCount="indefinite" begin="0.26s"/>
    <animate attributeName="y"      values="8;20;6;23;8"    dur="2.35s" repeatCount="indefinite" begin="0.26s"/>
  </rect>
  <!-- RIGHT half mirrors (360–600) -->
  <rect x="362" y="8"  width="4" height="64" rx="2" fill="url(#waveGrad)" opacity="0.65">
    <animate attributeName="height" values="64;40;68;34;64" dur="2.35s" repeatCount="indefinite" begin="0.26s"/>
    <animate attributeName="y"      values="8;20;6;23;8"    dur="2.35s" repeatCount="indefinite" begin="0.26s"/>
  </rect>
  <rect x="372" y="0"  width="4" height="80" rx="2" fill="url(#waveGrad)" opacity="0.65">
    <animate attributeName="height" values="80;56;80;52;80" dur="1.65s" repeatCount="indefinite" begin="0.23s"/>
    <animate attributeName="y"      values="0;12;0;14;0"    dur="1.65s" repeatCount="indefinite" begin="0.23s"/>
  </rect>
  <rect x="382" y="13" width="4" height="54" rx="2" fill="url(#waveGrad)" opacity="0.65">
    <animate attributeName="height" values="54;30;62;26;54" dur="2.15s" repeatCount="indefinite" begin="0.13s"/>
    <animate attributeName="y"      values="13;25;9;27;13"  dur="2.15s" repeatCount="indefinite" begin="0.13s"/>
  </rect>
  <rect x="392" y="4"  width="4" height="72" rx="2" fill="url(#waveGrad)" opacity="0.65">
    <animate attributeName="height" values="72;46;75;40;72" dur="1.95s" repeatCount="indefinite" begin="0.17s"/>
    <animate attributeName="y"      values="4;17;2;20;4"    dur="1.95s" repeatCount="indefinite" begin="0.17s"/>
  </rect>
  <rect x="402" y="7"  width="4" height="66" rx="2" fill="url(#waveGrad)" opacity="0.7">
    <animate attributeName="height" values="66;42;70;36;66" dur="2.4s" repeatCount="indefinite" begin="0.04s"/>
    <animate attributeName="y"      values="7;19;5;22;7"    dur="2.4s" repeatCount="indefinite" begin="0.04s"/>
  </rect>
  <rect x="412" y="2"  width="4" height="76" rx="2" fill="url(#waveGrad)" opacity="0.7">
    <animate attributeName="height" values="76;53;78;48;76" dur="1.5s" repeatCount="indefinite" begin="0.21s"/>
    <animate attributeName="y"      values="2;13;1;16;2"    dur="1.5s" repeatCount="indefinite" begin="0.21s"/>
  </rect>
  <rect x="422" y="11" width="4" height="58" rx="2" fill="url(#waveGrad)" opacity="0.7">
    <animate attributeName="height" values="58;34;66;28;58" dur="1.8s" repeatCount="indefinite" begin="0.11s"/>
    <animate attributeName="y"      values="11;23;7;26;11"  dur="1.8s" repeatCount="indefinite" begin="0.11s"/>
  </rect>
  <rect x="432" y="6"  width="4" height="68" rx="2" fill="url(#waveGrad)" opacity="0.7">
    <animate attributeName="height" values="68;44;72;40;68" dur="2.2s" repeatCount="indefinite" begin="0.16s"/>
    <animate attributeName="y"      values="6;18;4;20;6"    dur="2.2s" repeatCount="indefinite" begin="0.16s"/>
  </rect>
  <rect x="442" y="16" width="4" height="48" rx="2" fill="url(#waveGrad)" opacity="0.7">
    <animate attributeName="height" values="48;24;60;20;48" dur="2.0s" repeatCount="indefinite" begin="0.06s"/>
    <animate attributeName="y"      values="16;28;10;30;16" dur="2.0s" repeatCount="indefinite" begin="0.06s"/>
  </rect>
  <rect x="452" y="1"  width="4" height="78" rx="2" fill="url(#waveGrad)" opacity="0.7">
    <animate attributeName="height" values="78;55;79;50;78" dur="1.6s" repeatCount="indefinite" begin="0.24s"/>
    <animate attributeName="y"      values="1;12;0;15;1"    dur="1.6s" repeatCount="indefinite" begin="0.24s"/>
  </rect>
  <rect x="462" y="9"  width="4" height="62" rx="2" fill="url(#waveGrad)" opacity="0.7">
    <animate attributeName="height" values="62;38;70;32;62" dur="2.3s" repeatCount="indefinite" begin="0.09s"/>
    <animate attributeName="y"      values="9;21;5;24;9"    dur="2.3s" repeatCount="indefinite" begin="0.09s"/>
  </rect>
  <rect x="472" y="3"  width="4" height="74" rx="2" fill="url(#waveGrad)" opacity="0.7">
    <animate attributeName="height" values="74;50;76;46;74" dur="1.7s" repeatCount="indefinite" begin="0.19s"/>
    <animate attributeName="y"      values="3;15;2;17;3"    dur="1.7s" repeatCount="indefinite" begin="0.19s"/>
  </rect>
  <rect x="482" y="14" width="4" height="52" rx="2" fill="url(#waveGrad)" opacity="0.7">
    <animate attributeName="height" values="52;28;64;24;52" dur="2.1s" repeatCount="indefinite" begin="0.14s"/>
    <animate attributeName="y"      values="14;26;8;28;14"  dur="2.1s" repeatCount="indefinite" begin="0.14s"/>
  </rect>
  <rect x="492" y="5"  width="4" height="70" rx="2" fill="url(#waveGrad)" opacity="0.7">
    <animate attributeName="height" values="70;42;74;38;70" dur="1.9s" repeatCount="indefinite" begin="0.03s"/>
    <animate attributeName="y"      values="5;19;3;21;5"    dur="1.9s" repeatCount="indefinite" begin="0.03s"/>
  </rect>
  <rect x="502" y="18" width="4" height="44" rx="2" fill="url(#waveGrad)" opacity="0.85">
    <animate attributeName="height" values="44;70;20;74;44" dur="2.45s" repeatCount="indefinite" begin="0.07s"/>
    <animate attributeName="y"      values="18;5;30;3;18"   dur="2.45s" repeatCount="indefinite" begin="0.07s"/>
  </rect>
  <rect x="512" y="2"  width="4" height="76" rx="2" fill="url(#waveGrad)" opacity="0.85">
    <animate attributeName="height" values="76;52;78;46;76" dur="1.45s" repeatCount="indefinite" begin="0.22s"/>
    <animate attributeName="y"      values="2;14;1;17;2"    dur="1.45s" repeatCount="indefinite" begin="0.22s"/>
  </rect>
  <rect x="522" y="9"  width="4" height="62" rx="2" fill="url(#waveGrad)" opacity="0.85">
    <animate attributeName="height" values="62;38;70;32;62" dur="2.35s" repeatCount="indefinite" begin="0.18s"/>
    <animate attributeName="y"      values="9;21;5;24;9"    dur="2.35s" repeatCount="indefinite" begin="0.18s"/>
  </rect>
  <rect x="532" y="10" width="4" height="60" rx="2" fill="url(#waveGrad)" filter="url(#glow)" opacity="0.85">
    <animate attributeName="height" values="60;35;68;30;60" dur="2.35s" repeatCount="indefinite" begin="0.18s"/>
    <animate attributeName="y"      values="10;22;6;25;10"  dur="2.35s" repeatCount="indefinite" begin="0.18s"/>
  </rect>
  <rect x="542" y="3"  width="4" height="74" rx="2" fill="url(#waveGrad)" filter="url(#glow)" opacity="0.85">
    <animate attributeName="height" values="74;50;76;46;74" dur="1.75s" repeatCount="indefinite" begin="0.15s"/>
    <animate attributeName="y"      values="3;15;2;17;3"    dur="1.75s" repeatCount="indefinite" begin="0.15s"/>
  </rect>
  <rect x="552" y="8"  width="4" height="64" rx="2" fill="url(#waveGrad)" filter="url(#glow)" opacity="0.85">
    <animate attributeName="height" values="64;30;72;25;64" dur="2.05s" repeatCount="indefinite" begin="0.1s"/>
    <animate attributeName="y"      values="8;25;4;27;8"    dur="2.05s" repeatCount="indefinite" begin="0.1s"/>
  </rect>
  <rect x="562" y="20" width="4" height="40" rx="2" fill="url(#waveGrad)" filter="url(#glow)" opacity="0.85">
    <animate attributeName="height" values="40;60;22;70;40" dur="1.55s" repeatCount="indefinite" begin="0s"/>
    <animate attributeName="y"      values="20;10;29;5;20"  dur="1.55s" repeatCount="indefinite" begin="0s"/>
  </rect>
  <rect x="572" y="18" width="4" height="44" rx="2" fill="url(#waveGrad)" filter="url(#glow)" opacity="0.85">
    <animate attributeName="height" values="44;68;22;72;44" dur="1.6s" repeatCount="indefinite" begin="0.2s"/>
    <animate attributeName="y"      values="18;6;29;4;18"   dur="1.6s" repeatCount="indefinite" begin="0.2s"/>
  </rect>
  <rect x="582" y="15" width="4" height="50" rx="2" fill="url(#waveGrad)" filter="url(#glow)" opacity="0.85">
    <animate attributeName="height" values="50;22;66;18;50" dur="2.15s" repeatCount="indefinite" begin="0.08s"/>
    <animate attributeName="y"      values="15;29;7;31;15"  dur="2.15s" repeatCount="indefinite" begin="0.08s"/>
  </rect>
  <rect x="592" y="6"  width="4" height="68" rx="2" fill="url(#waveGrad)" filter="url(#glow)" opacity="0.85">
    <animate attributeName="height" values="68;40;76;35;68" dur="1.65s" repeatCount="indefinite" begin="0.2s"/>
    <animate attributeName="y"      values="6;20;2;22;6"    dur="1.65s" repeatCount="indefinite" begin="0.2s"/>
  </rect>
  <rect x="602" y="12" width="4" height="56" rx="2" fill="url(#waveGrad)" filter="url(#glow)" opacity="0.85">
    <animate attributeName="height" values="56;75;30;72;56" dur="2.25s" repeatCount="indefinite" begin="0.05s"/>
    <animate attributeName="y"      values="12;2;25;4;12"   dur="2.25s" repeatCount="indefinite" begin="0.05s"/>
  </rect>
</svg>

<br><br>

<!-- Animated typing badge -->
<a href="#">
  <img src="https://readme-typing-svg.demolab.com?font=JetBrains+Mono&weight=700&size=16&duration=2800&pause=900&color=7C5CFF&center=true&vCenter=true&multiline=false&width=520&lines=Windows+%E2%80%A2+WPF+%E2%80%A2+.NET+10;NAudio+%7C+WASAPI+%7C+VB-CABLE;%F0%9F%8E%99%EF%B8%8F+%E2%80%A2+Monitör+%E2%80%A2+Yayın+%E2%80%A2+Mikrofon+Mikseri;%F0%9F%8E%B5+%E2%80%A2+mp3+%C2%B7+wav+%C2%B7+ogg+%E2%80%A2+%F0%9F%94%81+Dalga+Formu+K%C4%B1rpma" alt="Typing SVG" />
</a>

<br><br>

<!-- Tech badges -->
![Windows](https://img.shields.io/badge/Windows-0078D6?style=for-the-badge&logo=windows&logoColor=white)
![dotnet](https://img.shields.io/badge/.NET_10-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![WPF](https://img.shields.io/badge/WPF-7C5CFF?style=for-the-badge&logo=windows&logoColor=white)
![NAudio](https://img.shields.io/badge/NAudio-2.2.1-8F73FF?style=for-the-badge)
![WASAPI](https://img.shields.io/badge/WASAPI-Shared_Mode-43D39E?style=for-the-badge)

<br>

</div>

---

## Özellikler

<div align="center">

| 🎧 Ses Motoru | 🎙️ Mikser | ✂️ Dalga Formu Editörü |
|:---:|:---:|:---:|
| Aynı anda sınırsız ses | Fiziksel mic → Sanal mikrofon | SVG tabanlı dalga formu önizleme |
| mp3 · wav · ogg | A (Kulaklık) + B (Yayın) çıkışı | Sürüklenebilir başlangıç/bitiş tutamaçları |
| WASAPI paylaşımlı mod | VB-CABLE otomatik algılama | Dahili önizleme oynatıcı + playhead |
| Ses seviyesi kontrolü | Otomatik karıştırma | Seçili aralığı `.wav` olarak dışa aktar |

</div>

<br>

```
+ Dosya Ekle        → tek/çoklu dosya veya sürükle-bırak
+ Klasör Tara       → alt klasörler dahil toplu tarama
⏹ Tümünü Durdur    → çalan her şeyi anında kes (panik butonu)
⌨ Global Kısayol   → her sese tek tuş veya Ctrl/Alt kombinasyonu
✂ Kırp & Kaydet    → aralığı yeni .wav dosyası olarak dışa aktar
🔍 Ctrl+F          → ada veya dosya adına anlık arama
```

---

## Kurulum

```powershell
# Çalıştır
dotnet run --project SoundDeck/SoundDeck.csproj

# Release derle
dotnet build SoundDeck/SoundDeck.csproj -c Release
```

> **Gereksinim:** .NET 10 SDK · Windows 10/11 64-bit

---

## Sanal Mikrofon (VB-CABLE)

1. [VB-Audio Cable](https://vb-audio.com/Cable/) adresinden indirin, **yönetici olarak** kurun.
2. SoundDeck'te **B → CABLE Input** seçin (veya **Sanal mikrofonu seç** butonuna basın).
3. Discord/OBS'de giriş cihazını **CABLE Output** yapın.

> **Mikrofon karıştırma:** `MİKROFON` satırında fiziksel mikrofonunuzu seçin ve anahtarı açın. SoundDeck ses efektleriyle birlikte mikrofonunuzu otomatik olarak B'ye karıştırır; ayrıca Windows "Dinle" ayarına gerek yoktur.

---

## Dalga Formu Kırpıcı

Düzenle penceresini açın → ses dosyasının dalga formu analiz edilir ve çizilir.

- **Sarı tutamaçları** sürükleyerek başlangıç/bitiş seçin.
- **▶ Aralığı Dinle** ile önizleyin; beyaz playhead ilerlemeyi gösterir.
- **✂ Sesi Kes ve Kaydet** ile seçili aralığı `.wav` olarak dışa aktarın.
- Başlangıç/bitiş saniye kutuları ve `0:00` / `Son` kısayol butonları ile hassas zamanlama.

---

## Ayar Dosyası

```
%AppData%\SoundDeck\sounddeck_config.json
```

Dosya yolları, görünen adlar, ses seviyeleri, kısayollar, çıkış cihazları, başlangıç/bitiş süreleri burada saklanır. Dosya bozulursa otomatik yedeklenir ve boş listeyle açılır.

---

## Teknik

| Bileşen | Detay |
|---|---|
| UI | WPF / .NET 10 |
| Ses | NAudio 2.2.1 + NAudio.Vorbis |
| Çıkış | WASAPI Shared Mode ~80 ms |
| Kısayollar | Win32 `RegisterHotKey` |
| Dalga analizi | Özel peak extraction (500–600 nokta) |
| Sanal mikrofon | VB-CABLE (harici, imzalı kernel sürücüsü) |

<div align="center">
<img src="https://capsule-render.vercel.app/api?type=waving&color=7C5CFF&height=100&section=footer" width="100%"/>
</div>
