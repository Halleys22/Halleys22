# Artwork del perfil

Diseño original inspirado en la exploración espacial: azul noche, verde bosque, un pequeño mundo con árboles y una hoguera, y un cometa que recorre su órbita.

## Archivos

- `assets/banner.gif`: portada de 1200 × 400 píxeles, 48 fotogramas, bucle de 6,72 segundos.
- `assets/banner-static.png`: alternativa estática para la preferencia de movimiento reducido.
- `assets/technologies.svg`: etiquetas vectoriales de las tecnologías de los proyectos.
- `BannerGenerator.cs`: código de dibujo y ensamblado de la animación.
- `build-assets.ps1`: regeneración de la portada en Windows con System.Drawing y Segoe UI.

Para regenerar los archivos desde una sesión de PowerShell que permita ejecutar scripts locales:

```powershell
.\build-assets.ps1
```

El diseño no usa imágenes del juego, servicios de estadísticas, contadores, rastreadores ni solicitudes de datos a terceros. El README enlaza directamente a los proyectos públicos y distingue el prototipo personal de los trabajos académicos.

La animación y la alternativa estática se sirven desde este mismo repositorio. La compatibilidad de la preferencia de movimiento reducido depende del navegador del visitante.

