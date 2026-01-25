<h1>Nuggate</h1>

Tu servidor privado de paquetes NuGet — simple, seguro y autoalojado

<p>
    <img src="https://img.shields.io/badge/License-GPLv3-blue.svg" />
    <img src="https://img.shields.io/badge/Built%20with-.NET%209.0-512BD4?logo=dotnet&logoColor=white" />
    <img src="https://img.shields.io/badge/Private%20NuGet-Server-success" />
    <img src="https://img.shields.io/badge/Platform-Linux%20%7C%20Windows-orange" />
</p>

---

## 🧩 ¿Qué es Nuggate?

**Nuggate** es un servidor **ligero y autoalojado** para hospedar tus propios paquetes **NuGet privados**.
Ideal para equipos y empresas que necesitan mantener bibliotecas internas sin depender de servidores públicos o GitHub Packages.

💡 Diseñado para ser desplegado en minutos — sin configuraciones complicadas ni infraestructura externa.

---

## 🚀 Características

- **Ligero** — Web API .NET 9 sin dependencias innecesarias  
- **Privado y seguro** — control total del acceso a tus paquetes  
- **Autoalojado** — en tu servidor con pubicación de api o en un contenedor Docker  
- **Simple de configurar** — archivos planos en almacenamiento local del servidor

---

## ⚙️ Instalación 

### Opción 1: Manual (.NET 9 SDK requerido)
1. Clona este repositorio y ejecuta los siguientes comandos:
```bash
    git clone https://github.com/sepulvedamarcos/nuggate.git
    cd nuggate
    dotnet restore
    dotneT publish --configuration Release --output ./publish
```

2. Empaqueta la aplicación en la carpeta `publish` y publica en tu servidor.

### Opción 2: Docker (proximamente)


## 🖼️ Captura swagger autodocumentado

Aquí puedes ver una imagen del swagger desplegado con los endpoint con sus verbos a copnsumir.

![Nuggate Screenshot](nuggate_api.png)

## Como usar en tu aplicación
Debe localizar el archivo **NuGet.config** que esta en ubicado en:
  - **LINUX** /home/usuario/.nuget/NuGet/
  - **Windows** C:\Users\TuUsuario\AppData\Roaming\NuGet\NuGet.Config

Luego agregar la siguiente linea:
```bash
<add key="nuggate.org" value="https://api.tudominio.cl/api/nuggate/index.json" protocolVersion="3" />
```


---

## 🤝 Contribuir

¡Las contribuciones son bienvenidas! Ya sea reportando bugs, sugiriendo funcionalidades o enviando código.

1. Has un **Fork** del proyecto
2. Crea tu branch (`git checkout -b feature/nueva-funcionalidad`)
3. Commit tus cambios (`git commit -m 'Agrega nueva funcionalidad'`)
4. Push al branch (`git push origin feature/nueva-funcionalidad`)
5. Abre un Pull Request

---

### 👥 Conectemos

<p align="center">
  <a href="https://www.linkedin.com/in/sepulveda-marcos">
    <img src="https://img.shields.io/badge/LinkedIn-Marcos%20Sep%C3%BAlveda-blue?logo=linkedin&logoColor=white" />
  </a>
  <a href="mailto:sepulvedamarcos@gmail.com">
    <img src="https://img.shields.io/badge/Email-sepulvedamarcos%40gmail.com-red?logo=gmail&logoColor=white" />
  </a>
  <a href="https://ko-fi.com/sepulvedamarcos">
    <img src="https://img.shields.io/badge/Ko--fi-Apoyar%20con%20un%20caf%C3%A9-ff5e5b?logo=kofi&logoColor=white" />
  </a>
</p>

---

**¿Te gusta Nuggate?**

⭐ Dale una estrella al repo y cuéntanos tu experiencia 

---
<p align="center">
  <i>"Una aplicación no debería obligarte a reorganizar tu vida digital. Debería adaptarse a como ya trabajas."</i>
</p>
