<div align="center">

<!-- HEADER SVG – GitHub'da filter/linearGradient çalışmaz, düz renk kullanıyoruz -->
<svg xmlns="[http://www.w3.org/2000/svg](https://ishan-res.vercel.app/svg/banner/dev/cesurefe6355)" width="900" height="220" viewBox="0 0 900 220">
  <!-- Arka plan -->
  <rect width="900" height="220" fill="#0d1117"/>

  <!-- Alt dalga şekli (dekoratif) -->
  <path d="M0,180 Q100,140 200,170 Q300,200 400,155 Q500,110 600,165 Q700,200 800,150 Q860,120 900,140 L900,220 L0,220 Z" fill="#7C5CFF" opacity="0.15"/>
  <path d="M0,190 Q120,160 240,185 Q360,205 480,170 Q600,135 720,178 Q810,205 900,175 L900,220 L0,220 Z" fill="#7C5CFF" opacity="0.25"/>

  <!-- Başlık metni -->
  <text x="450" y="90" font-family="'Segoe UI', Arial, sans-serif" font-size="64" font-weight="900" fill="#ffffff" text-anchor="middle" letter-spacing="2">SoundDeck</text>
  <text x="450" y="125" font-family="'Segoe UI', Arial, sans-serif" font-size="18" fill="#9B8AFF" text-anchor="middle" letter-spacing="1">Ses Panosu &amp; Yayın Mikseri</text>

  <!-- Dekoratif çizgi -->
  <line x1="310" y1="140" x2="590" y2="140" stroke="#7C5CFF" stroke-width="1" opacity="0.6"/>
  <circle cx="450" cy="140" r="3" fill="#7C5CFF"/>
</svg>

<!-- CANLI WAVEFORM ANİMASYONU – filter yok, url() referansı yok, salt katı renkler -->
<svg xmlns="http://www.w3.org/2000/svg" width="860" height="80" viewBox="0 0 860 80">

  <!-- Sol fade barları -->
  <rect x="0"  y="35" width="4" height="10" rx="2" fill="#7C5CFF" opacity="0.25"><animate attributeName="height" values="10;22;8;18;10" dur="1.8s" repeatCount="indefinite"/><animate attributeName="y" values="35;29;36;31;35" dur="1.8s" repeatCount="indefinite"/></rect>
  <rect x="8"  y="28" width="4" height="24" rx="2" fill="#7C5CFF" opacity="0.40"><animate attributeName="height" values="24;12;36;16;24" dur="2.1s" repeatCount="indefinite"/><animate attributeName="y" values="28;34;22;32;28" dur="2.1s" repeatCount="indefinite"/></rect>
  <rect x="16" y="18" width="4" height="44" rx="2" fill="#7C5CFF" opacity="0.55"><animate attributeName="height" values="44;28;58;22;44" dur="1.5s" repeatCount="indefinite"/><animate attributeName="y" values="18;26;11;29;18" dur="1.5s" repeatCount="indefinite"/></rect>
  <rect x="24" y="10" width="4" height="60" rx="2" fill="#7C5CFF" opacity="0.70"><animate attributeName="height" values="60;40;70;34;60" dur="2.4s" repeatCount="indefinite"/><animate attributeName="y" values="10;20;5;23;10" dur="2.4s" repeatCount="indefinite"/></rect>
  <rect x="32" y="5"  width="4" height="70" rx="2" fill="#7C5CFF" opacity="0.85"><animate attributeName="height" values="70;50;74;42;70" dur="1.9s" repeatCount="indefinite"/><animate attributeName="y" values="5;15;3;19;5" dur="1.9s" repeatCount="indefinite"/></rect>
  <rect x="40" y="4"  width="4" height="72" rx="2" fill="#7C5CFF" opacity="0.95"><animate attributeName="height" values="72;44;76;38;72" dur="2.2s" repeatCount="indefinite"/><animate attributeName="y" values="4;18;2;21;4" dur="2.2s" repeatCount="indefinite"/></rect>

  <!-- Sol-merkez yoğun alan -->
  <rect x="52"  y="12" width="4" height="56" rx="2" fill="#8B6FFF"><animate attributeName="height" values="56;72;30;68;56" dur="1.55s" repeatCount="indefinite" begin="0s"/><animate attributeName="y" values="12;4;25;6;12" dur="1.55s" repeatCount="indefinite" begin="0s"/></rect>
  <rect x="60"  y="6"  width="4" height="68" rx="2" fill="#9B80FF"><animate attributeName="height" values="68;40;74;32;68" dur="2.05s" repeatCount="indefinite" begin="0.1s"/><animate attributeName="y" values="6;20;3;24;6" dur="2.05s" repeatCount="indefinite" begin="0.1s"/></rect>
  <rect x="68"  y="2"  width="4" height="76" rx="2" fill="#7C5CFF"><animate attributeName="height" values="76;54;78;48;76" dur="1.75s" repeatCount="indefinite" begin="0.15s"/><animate attributeName="y" values="2;13;1;16;2" dur="1.75s" repeatCount="indefinite" begin="0.15s"/></rect>
  <rect x="76"  y="8"  width="4" height="64" rx="2" fill="#8B6FFF"><animate attributeName="height" values="64;76;28;72;64" dur="2.35s" repeatCount="indefinite" begin="0.2s"/><animate attributeName="y" values="8;2;26;4;8" dur="2.35s" repeatCount="indefinite" begin="0.2s"/></rect>
  <rect x="84"  y="4"  width="4" height="72" rx="2" fill="#7C5CFF"><animate attributeName="height" values="72;48;76;40;72" dur="1.65s" repeatCount="indefinite" begin="0.05s"/><animate attributeName="y" values="4;16;2;20;4" dur="1.65s" repeatCount="indefinite" begin="0.05s"/></rect>
  <rect x="92"  y="14" width="4" height="52" rx="2" fill="#9B80FF"><animate attributeName="height" values="52;70;24;66;52" dur="2.15s" repeatCount="indefinite" begin="0.3s"/><animate attributeName="y" values="14;5;28;7;14" dur="2.15s" repeatCount="indefinite" begin="0.3s"/></rect>
  <rect x="100" y="8"  width="4" height="64" rx="2" fill="#7C5CFF"><animate attributeName="height" values="64;36;72;30;64" dur="1.85s" repeatCount="indefinite" begin="0.4s"/><animate attributeName="y" values="8;22;4;25;8" dur="1.85s" repeatCount="indefinite" begin="0.4s"/></rect>
  <rect x="108" y="3"  width="4" height="74" rx="2" fill="#8B6FFF"><animate attributeName="height" values="74;52;78;44;74" dur="1.45s" repeatCount="indefinite" begin="0.25s"/><animate attributeName="y" values="3;14;1;18;3" dur="1.45s" repeatCount="indefinite" begin="0.25s"/></rect>
  <rect x="116" y="10" width="4" height="60" rx="2" fill="#7C5CFF"><animate attributeName="height" values="60;76;22;74;60" dur="2.25s" repeatCount="indefinite" begin="0.35s"/><animate attributeName="y" values="10;2;29;3;10" dur="2.25s" repeatCount="indefinite" begin="0.35s"/></rect>
  <rect x="124" y="5"  width="4" height="70" rx="2" fill="#9B80FF"><animate attributeName="height" values="70;42;76;36;70" dur="1.95s" repeatCount="indefinite" begin="0.45s"/><animate attributeName="y" values="5;19;2;22;5" dur="1.95s" repeatCount="indefinite" begin="0.45s"/></rect>

  <!-- Merkez peak – en yüksek alan (SoundDeck yazısının altı) -->
  <rect x="138" y="2"  width="5" height="76" rx="2.5" fill="#7C5CFF"><animate attributeName="height" values="76;56;78;50;76" dur="1.6s" repeatCount="indefinite" begin="0s"/><animate attributeName="y" values="2;12;1;15;2" dur="1.6s" repeatCount="indefinite" begin="0s"/></rect>
  <rect x="147" y="1"  width="5" height="78" rx="2.5" fill="#8B6FFF"><animate attributeName="height" values="78;60;79;52;78" dur="2.0s" repeatCount="indefinite" begin="0.08s"/><animate attributeName="y" values="1;10;0;14;1" dur="2.0s" repeatCount="indefinite" begin="0.08s"/></rect>
  <rect x="156" y="0"  width="5" height="80" rx="2.5" fill="#9B80FF"><animate attributeName="height" values="80;62;79;55;80" dur="1.4s" repeatCount="indefinite" begin="0.16s"/><animate attributeName="y" values="0;9;1;12;0" dur="1.4s" repeatCount="indefinite" begin="0.16s"/></rect>
  <rect x="165" y="1"  width="5" height="78" rx="2.5" fill="#7C5CFF"><animate attributeName="height" values="78;58;80;50;78" dur="1.85s" repeatCount="indefinite" begin="0.12s"/><animate attributeName="y" values="1;11;0;15;1" dur="1.85s" repeatCount="indefinite" begin="0.12s"/></rect>
  <rect x="174" y="3"  width="5" height="74" rx="2.5" fill="#8B6FFF"><animate attributeName="height" values="74;54;78;46;74" dur="2.3s" repeatCount="indefinite" begin="0.2s"/><animate attributeName="y" values="3;13;1;17;3" dur="2.3s" repeatCount="indefinite" begin="0.2s"/></rect>
  <rect x="183" y="0"  width="5" height="80" rx="2.5" fill="#7C5CFF"><animate attributeName="height" values="80;60;78;53;80" dur="1.5s" repeatCount="indefinite" begin="0.04s"/><animate attributeName="y" values="0;10;1;13;0" dur="1.5s" repeatCount="indefinite" begin="0.04s"/></rect>
  <rect x="192" y="2"  width="5" height="76" rx="2.5" fill="#9B80FF"><animate attributeName="height" values="76;50;80;44;76" dur="2.1s" repeatCount="indefinite" begin="0.28s"/><animate attributeName="y" values="2;15;0;18;2" dur="2.1s" repeatCount="indefinite" begin="0.28s"/></rect>
  <rect x="201" y="4"  width="5" height="72" rx="2.5" fill="#7C5CFF"><animate attributeName="height" values="72;48;76;42;72" dur="1.7s" repeatCount="indefinite" begin="0.36s"/><animate attributeName="y" values="4;16;2;19;4" dur="1.7s" repeatCount="indefinite" begin="0.36s"/></rect>
  <rect x="210" y="1"  width="5" height="78" rx="2.5" fill="#8B6FFF"><animate attributeName="height" values="78;56;80;49;78" dur="1.55s" repeatCount="indefinite" begin="0.44s"/><animate attributeName="y" values="1;12;0;15;1" dur="1.55s" repeatCount="indefinite" begin="0.44s"/></rect>
  <rect x="219" y="3"  width="5" height="74" rx="2.5" fill="#7C5CFF"><animate attributeName="height" values="74;52;78;44;74" dur="2.2s" repeatCount="indefinite" begin="0.52s"/><animate attributeName="y" values="3;14;1;18;3" dur="2.2s" repeatCount="indefinite" begin="0.52s"/></rect>

  <!-- Sağ-merkez -->
  <rect x="232" y="8"  width="4" height="64" rx="2" fill="#7C5CFF"><animate attributeName="height" values="64;40;70;34;64" dur="1.9s" repeatCount="indefinite" begin="0.6s"/><animate attributeName="y" values="8;20;5;23;8" dur="1.9s" repeatCount="indefinite" begin="0.6s"/></rect>
  <rect x="240" y="4"  width="4" height="72" rx="2" fill="#8B6FFF"><animate attributeName="height" values="72;46;76;38;72" dur="2.4s" repeatCount="indefinite" begin="0.5s"/><animate attributeName="y" values="4;17;2;21;4" dur="2.4s" repeatCount="indefinite" begin="0.5s"/></rect>
  <rect x="248" y="10" width="4" height="60" rx="2" fill="#7C5CFF"><animate attributeName="height" values="60;74;26;70;60" dur="1.65s" repeatCount="indefinite" begin="0.55s"/><animate attributeName="y" values="10;3;27;5;10" dur="1.65s" repeatCount="indefinite" begin="0.55s"/></rect>
  <rect x="256" y="6"  width="4" height="68" rx="2" fill="#9B80FF"><animate attributeName="height" values="68;42;74;36;68" dur="2.05s" repeatCount="indefinite" begin="0.65s"/><animate attributeName="y" values="6;19;3;22;6" dur="2.05s" repeatCount="indefinite" begin="0.65s"/></rect>
  <rect x="264" y="3"  width="4" height="74" rx="2" fill="#7C5CFF"><animate attributeName="height" values="74;50;78;42;74" dur="1.75s" repeatCount="indefinite" begin="0.7s"/><animate attributeName="y" values="3;15;1;19;3" dur="1.75s" repeatCount="indefinite" begin="0.7s"/></rect>
  <rect x="272" y="8"  width="4" height="64" rx="2" fill="#8B6FFF"><animate attributeName="height" values="64;76;28;72;64" dur="2.35s" repeatCount="indefinite" begin="0.75s"/><animate attributeName="y" values="8;2;26;4;8" dur="2.35s" repeatCount="indefinite" begin="0.75s"/></rect>
  <rect x="280" y="5"  width="4" height="70" rx="2" fill="#7C5CFF"><animate attributeName="height" values="70;44;76;38;70" dur="1.55s" repeatCount="indefinite" begin="0.8s"/><animate attributeName="y" values="5;18;2;21;5" dur="1.55s" repeatCount="indefinite" begin="0.8s"/></rect>
  <rect x="288" y="12" width="4" height="56" rx="2" fill="#9B80FF"><animate attributeName="height" values="56;70;24;66;56" dur="2.15s" repeatCount="indefinite" begin="0.85s"/><animate attributeName="y" values="12;5;28;7;12" dur="2.15s" repeatCount="indefinite" begin="0.85s"/></rect>
  <rect x="296" y="7"  width="4" height="66" rx="2" fill="#7C5CFF"><animate attributeName="height" values="66;38;72;32;66" dur="1.85s" repeatCount="indefinite" begin="0.9s"/><animate attributeName="y" values="7;21;4;24;7" dur="1.85s" repeatCount="indefinite" begin="0.9s"/></rect>
  <rect x="304" y="14" width="4" height="52" rx="2" fill="#8B6FFF"><animate attributeName="height" values="52;68;22;64;52" dur="2.25s" repeatCount="indefinite" begin="0.95s"/><animate attributeName="y" values="14;6;29;8;14" dur="2.25s" repeatCount="indefinite" begin="0.95s"/></rect>

  <!-- SAĞ YOĞUN ALAN – simetrik merkez -->
  <rect x="318" y="2"  width="5" height="76" rx="2.5" fill="#7C5CFF"><animate attributeName="height" values="76;52;80;44;76" dur="1.6s" repeatCount="indefinite" begin="1.0s"/><animate attributeName="y" values="2;14;0;18;2" dur="1.6s" repeatCount="indefinite" begin="1.0s"/></rect>
  <rect x="327" y="0"  width="5" height="80" rx="2.5" fill="#8B6FFF"><animate attributeName="height" values="80;58;78;50;80" dur="2.0s" repeatCount="indefinite" begin="1.05s"/><animate attributeName="y" values="0;11;1;15;0" dur="2.0s" repeatCount="indefinite" begin="1.05s"/></rect>
  <rect x="336" y="1"  width="5" height="78" rx="2.5" fill="#9B80FF"><animate attributeName="height" values="78;56;80;48;78" dur="1.4s" repeatCount="indefinite" begin="1.1s"/><animate attributeName="y" values="1;12;0;16;1" dur="1.4s" repeatCount="indefinite" begin="1.1s"/></rect>
  <rect x="345" y="3"  width="5" height="74" rx="2.5" fill="#7C5CFF"><animate attributeName="height" values="74;50;78;42;74" dur="1.85s" repeatCount="indefinite" begin="1.15s"/><animate attributeName="y" values="3;15;1;19;3" dur="1.85s" repeatCount="indefinite" begin="1.15s"/></rect>
  <rect x="354" y="0"  width="5" height="80" rx="2.5" fill="#8B6FFF"><animate attributeName="height" values="80;62;78;54;80" dur="2.3s" repeatCount="indefinite" begin="1.2s"/><animate attributeName="y" values="0;9;1;13;0" dur="2.3s" repeatCount="indefinite" begin="1.2s"/></rect>
  <rect x="363" y="2"  width="5" height="76" rx="2.5" fill="#7C5CFF"><animate attributeName="height" values="76;48;80;40;76" dur="1.5s" repeatCount="indefinite" begin="1.25s"/><animate attributeName="y" values="2;16;0;20;2" dur="1.5s" repeatCount="indefinite" begin="1.25s"/></rect>
  <rect x="372" y="4"  width="5" height="72" rx="2.5" fill="#9B80FF"><animate attributeName="height" values="72;50;76;44;72" dur="2.1s" repeatCount="indefinite" begin="1.3s"/><animate attributeName="y" values="4;15;2;18;4" dur="2.1s" repeatCount="indefinite" begin="1.3s"/></rect>
  <rect x="381" y="1"  width="5" height="78" rx="2.5" fill="#7C5CFF"><animate attributeName="height" values="78;54;80;46;78" dur="1.7s" repeatCount="indefinite" begin="1.35s"/><animate attributeName="y" values="1;13;0;17;1" dur="1.7s" repeatCount="indefinite" begin="1.35s"/></rect>
  <rect x="390" y="3"  width="5" height="74" rx="2.5" fill="#8B6FFF"><animate attributeName="height" values="74;46;78;38;74" dur="1.55s" repeatCount="indefinite" begin="1.4s"/><animate attributeName="y" values="3;17;1;21;3" dur="1.55s" repeatCount="indefinite" begin="1.4s"/></rect>
  <rect x="399" y="2"  width="5" height="76" rx="2.5" fill="#7C5CFF"><animate attributeName="height" values="76;52;80;44;76" dur="2.2s" repeatCount="indefinite" begin="1.45s"/><animate attributeName="y" values="2;14;0;18;2" dur="2.2s" repeatCount="indefinite" begin="1.45s"/></rect>

  <!-- Sağ azalan -->
  <rect x="412" y="8"  width="4" height="64" rx="2" fill="#7C5CFF"><animate attributeName="height" values="64;40;70;34;64" dur="1.9s" repeatCount="indefinite" begin="1.5s"/><animate attributeName="y" values="8;20;5;23;8" dur="1.9s" repeatCount="indefinite" begin="1.5s"/></rect>
  <rect x="420" y="5"  width="4" height="70" rx="2" fill="#8B6FFF"><animate attributeName="height" values="70;44;74;36;70" dur="2.4s" repeatCount="indefinite" begin="1.45s"/><animate attributeName="y" values="5;18;3;22;5" dur="2.4s" repeatCount="indefinite" begin="1.45s"/></rect>
  <rect x="428" y="12" width="4" height="56" rx="2" fill="#7C5CFF"><animate attributeName="height" values="56;72;24;68;56" dur="1.65s" repeatCount="indefinite" begin="1.55s"/><animate attributeName="y" values="12;4;28;6;12" dur="1.65s" repeatCount="indefinite" begin="1.55s"/></rect>
  <rect x="436" y="6"  width="4" height="68" rx="2" fill="#9B80FF"><animate attributeName="height" values="68;42;72;36;68" dur="2.05s" repeatCount="indefinite" begin="1.6s"/><animate attributeName="y" values="6;19;4;22;6" dur="2.05s" repeatCount="indefinite" begin="1.6s"/></rect>
  <rect x="444" y="3"  width="4" height="74" rx="2" fill="#7C5CFF"><animate attributeName="height" values="74;50;76;42;74" dur="1.75s" repeatCount="indefinite" begin="1.65s"/><animate attributeName="y" values="3;15;2;19;3" dur="1.75s" repeatCount="indefinite" begin="1.65s"/></rect>
  <rect x="452" y="10" width="4" height="60" rx="2" fill="#8B6FFF"><animate attributeName="height" values="60;74;28;70;60" dur="2.35s" repeatCount="indefinite" begin="1.7s"/><animate attributeName="y" values="10;3;26;5;10" dur="2.35s" repeatCount="indefinite" begin="1.7s"/></rect>
  <rect x="460" y="4"  width="4" height="72" rx="2" fill="#7C5CFF"><animate attributeName="height" values="72;46;74;38;72" dur="1.55s" repeatCount="indefinite" begin="1.75s"/><animate attributeName="y" values="4;17;3;21;4" dur="1.55s" repeatCount="indefinite" begin="1.75s"/></rect>
  <rect x="468" y="14" width="4" height="52" rx="2" fill="#9B80FF"><animate attributeName="height" values="52;66;22;62;52" dur="2.15s" repeatCount="indefinite" begin="1.8s"/><animate attributeName="y" values="14;7;29;9;14" dur="2.15s" repeatCount="indefinite" begin="1.8s"/></rect>
  <rect x="476" y="7"  width="4" height="66" rx="2" fill="#7C5CFF"><animate attributeName="height" values="66;38;70;30;66" dur="1.85s" repeatCount="indefinite" begin="1.85s"/><animate attributeName="y" values="7;21;5;25;7" dur="1.85s" repeatCount="indefinite" begin="1.85s"/></rect>
  <rect x="484" y="10" width="4" height="60" rx="2" fill="#8B6FFF"><animate attributeName="height" values="60;76;26;72;60" dur="2.25s" repeatCount="indefinite" begin="1.9s"/><animate attributeName="y" values="10;2;27;4;10" dur="2.25s" repeatCount="indefinite" begin="1.9s"/></rect>

  <!-- Sağ kenarlık azalan -->
  <rect x="498" y="4"  width="4" height="72" rx="2" fill="#7C5CFF" opacity="0.95"><animate attributeName="height" values="72;44;76;38;72" dur="2.2s" repeatCount="indefinite" begin="2.0s"/><animate attributeName="y" values="4;18;2;21;4" dur="2.2s" repeatCount="indefinite" begin="2.0s"/></rect>
  <rect x="506" y="5"  width="4" height="70" rx="2" fill="#7C5CFF" opacity="0.85"><animate attributeName="height" values="70;50;74;42;70" dur="1.9s" repeatCount="indefinite" begin="1.95s"/><animate attributeName="y" values="5;15;3;19;5" dur="1.9s" repeatCount="indefinite" begin="1.95s"/></rect>
  <rect x="514" y="10" width="4" height="60" rx="2" fill="#7C5CFF" opacity="0.70"><animate attributeName="height" values="60;40;68;34;60" dur="2.4s" repeatCount="indefinite" begin="2.1s"/><animate attributeName="y" values="10;20;6;23;10" dur="2.4s" repeatCount="indefinite" begin="2.1s"/></rect>
  <rect x="522" y="18" width="4" height="44" rx="2" fill="#7C5CFF" opacity="0.55"><animate attributeName="height" values="44;28;56;22;44" dur="1.5s" repeatCount="indefinite" begin="2.05s"/><animate attributeName="y" values="18;26;12;29;18" dur="1.5s" repeatCount="indefinite" begin="2.05s"/></rect>
  <rect x="530" y="28" width="4" height="24" rx="2" fill="#7C5CFF" opacity="0.40"><animate attributeName="height" values="24;12;36;16;24" dur="2.1s" repeatCount="indefinite" begin="2.15s"/><animate attributeName="y" values="28;34;22;32;28" dur="2.1s" repeatCount="indefinite" begin="2.15s"/></rect>
  <rect x="538" y="35" width="4" height="10" rx="2" fill="#7C5CFF" opacity="0.25"><animate attributeName="height" values="10;22;8;18;10" dur="1.8s" repeatCount="indefinite" begin="2.2s"/><animate attributeName="y" values="35;29;36;31;35" dur="1.8s" repeatCount="indefinite" begin="2.2s"/></rect>

</svg>

<br>

[![Typing SVG](https://readme-typing-svg.demolab.com?font=JetBrains+Mono&weight=700&size=16&duration=2800&pause=900&color=7C5CFF&center=true&vCenter=true&width=520&lines=Windows+%E2%80%A2+WPF+%E2%80%A2+.NET+10;NAudio+%7C+WASAPI+%7C+VB-CABLE;%F0%9F%8E%99%EF%B8%8F+Monit%C3%B6r+%E2%80%A2+Yay%C4%B1n+%E2%80%A2+Mikrofon+Mikseri;%F0%9F%8E%B5+mp3+%C2%B7+wav+%C2%B7+ogg+%E2%80%A2+%E2%9C%82%EF%B8%8F+Dalga+Formu+K%C4%B1rpma)](https://github.com)

<br>

![Windows](https://img.shields.io/badge/Windows-0078D6?style=for-the-badge&logo=windows&logoColor=white)
![dotnet](https://img.shields.io/badge/.NET_10-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![WPF](https://img.shields.io/badge/WPF-7C5CFF?style=for-the-badge&logo=windows&logoColor=white)
![NAudio](https://img.shields.io/badge/NAudio-2.2.1-8F73FF?style=for-the-badge)
![WASAPI](https://img.shields.io/badge/WASAPI-Shared_Mode-43D39E?style=for-the-badge)

</div>

---

## Özellikler

<div align="center">

| 🧠 Ses Motoru | 🎙️ Mikser | ✂️ Dalga Formu Editörü |
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

- **Sarı tutamaçları** sürükleyerek başlangıç/bitiş seçin
- **▶ Aralığı Dinle** ile önizleyin; beyaz playhead ilerlemeyi gösterir
- **✂ Sesi Kes ve Kaydet** ile seçili aralığı `.wav` olarak dışa aktarın
- Hassas zamanlama için `0:00` / `Son` kısayol butonları ve saniye kutuları

---

## Ayar Dosyası

```
%AppData%\SoundDeck\sounddeck_config.json
```

Dosya yolları, görünen adlar, ses seviyeleri, kısayollar, çıkış cihazları ve başlangıç/bitiş süreleri burada saklanır. Dosya bozulursa otomatik yedeklenir.

---

## Teknik

| Bileşen | Detay |
|---|---|
| UI | WPF / .NET 10 |
| Ses | NAudio 2.2.1 + NAudio.Vorbis |
| Çıkış | WASAPI Shared Mode ~80 ms |
| Kısayollar | Win32 `RegisterHotKey` |
| Dalga analizi | Özel peak extraction (600 nokta) |
| Sanal mikrofon | VB-CABLE (harici, imzalı kernel sürücüsü) |

<div align="center">
<br>

<!-- Footer SVG -->
<svg xmlns="http://www.w3.org/2000/svg" width="900" height="60" viewBox="0 0 900 60">
  <rect width="900" height="60" fill="#0d1117"/>
  <path d="M0,20 Q150,5 300,18 Q450,32 600,15 Q750,2 900,20 L900,60 L0,60 Z" fill="#7C5CFF" opacity="0.20"/>
  <path d="M0,30 Q180,15 360,28 Q540,42 720,22 Q810,12 900,30 L900,60 L0,60 Z" fill="#7C5CFF" opacity="0.35"/>
  <text x="450" y="50" font-family="'Segoe UI', Arial, sans-serif" font-size="11" fill="#9B8AFF" text-anchor="middle" opacity="0.7">SoundDeck · WPF · .NET 10 · NAudio</text>
</svg>

</div>
