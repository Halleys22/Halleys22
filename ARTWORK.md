# Artwork del perfil

Dise�o original inspirado en la exploraci�n espacial: azul noche, verde bosque, un peque�o mundo con �rboles y una hoguera, y un cometa que recorre su �rbita.

## Archivos

- `assets/banner.gif`: portada de 1200 � 400 p�xeles, 48 fotogramas, bucle de 6,72 segundos.
- `assets/banner-static.png`: alternativa est�tica para la preferencia de movimiento reducido.
- `assets/technologies.svg`: etiquetas vectoriales de las tecnolog�as de los proyectos.
- `BannerGenerator.cs`: c�digo de dibujo y ensamblado de la animaci�n.
- `build-assets.ps1`: regeneraci�n de la portada en Windows con System.Drawing y Segoe UI.

Para regenerar los archivos desde una sesi�n de PowerShell que permita ejecutar scripts locales:

```powershell
.\build-assets.ps1
```

El dise�o no usa im�genes del juego, servicios de estad�sticas, contadores, rastreadores ni solicitudes de datos a terceros. El README enlaza directamente a los proyectos p�blicos y distingue el prototipo personal de los trabajos acad�micos.

La animaci�n y la alternativa est�tica se sirven desde este mismo repositorio. La compatibilidad de la preferencia de movimiento reducido depende del navegador del visitante.

