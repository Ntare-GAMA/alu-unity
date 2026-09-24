# Unity - AR Business Card

A marker-based AR business card built with Unity 6000.4.5f1 and Vuforia Engine 11.4.4.
Point the phone camera at the printed marker: the card pops up from the marker, stands up,
and the header and link buttons animate in. Tapping a button plays a click, bounces the button,
and opens the link.

![Layout](Screenshots/BusinessCardLayout.png)

## Markers

The app recognizes two markers. The card appears on whichever one the camera sees:

- [Marker/HBTNARMarker.png](Marker/HBTNARMarker.png): the default Holberton marker from the project.
- [Marker/ARBusinessCardMarker.png](Marker/ARBusinessCardMarker.png): a custom marker. It uses high-contrast, non-repeating shapes with many sharp corners so Vuforia can detect it reliably.

Print a marker, or show it full screen on another device.

## Tasks

| Task | Where |
| --- | --- |
| 0. Layout | `0-layout` (link to `Screenshots/BusinessCardLayout.png`) |
| 1. Target acquired | `ARMarkerTarget` creates an image target for each marker when Vuforia starts. It anchors the card to the marker in view and shows the card only while that target is `TRACKED`. |
| 2. Animated reality | `BusinessCardAnimator`: the card pops up and tilts to 55°, the accent bar sweeps open, the header slides in, the buttons pop in one by one, then the card floats gently while idle |
| 3. Social link up | `LinkButton`: each button opens its link with `Application.OpenURL` (`mailto:`, GitHub, X, LinkedIn). Pressing gives a color tint, a squash-and-bounce, and a click sound. |
| 4. Builds | `Builds/Android/ARBusinessCard.apk`, `Builds/iOS/` (Xcode project) |

Android requires API 29 or higher (a Vuforia 11 requirement). IL2CPP, ARM64.

Accessibility: the white text on the buttons meets WCAG AA contrast. Every button has a text label, not only an icon or a color. Buttons are large and well spaced for touch. The idle motion is small, so the text stays readable.

## Project structure

```
Assets/
  Scenes/ARBusinessCard.unity
  Scripts/        ARMarkerTarget.cs, BusinessCardAnimator.cs, LinkButton.cs
  Editor/         ARBusinessCardBuilder.cs (generates the scene and the builds)
  Textures/       marker + Icons/
  Audio/          ButtonClick.wav
Marker/           printable marker
Screenshots/      layout screenshot
```

## Setup

1. Get the Vuforia package. The tarball is 138 MB, which is over GitHub's file size limit, so it is git-ignored.
   Import `add-vuforia-package-11-4-4.unitypackage` from developer.vuforia.com/downloads/sdk,
   or copy `com.ptc.vuforia.engine-11.4.4.tgz` into `Packages/`.
2. Paste your Vuforia license key in **Edit > Project Settings > Vuforia Engine**.
   `Assets/Resources/VuforiaConfiguration.asset` is git-ignored so the key stays private.
3. Open `Assets/Scenes/ARBusinessCard.unity`.

The card text and links live in the constants at the top of `Assets/Editor/ARBusinessCardBuilder.cs`.
After editing them, run **AR Business Card > Build Scene**. Use **Build Android** or **Build iOS** to rebuild the players.

Builds:
Android_apk : https://drive.google.com/file/d/1z5r1sPSsjwriWARwJfcFaERm0YgPq0fQ/view?usp=sharing
iOS : https://drive.google.com/file/d/1VAla48P2rWTMPxHjNGa45Ti4H-XwdHSu/view?usp=sharing
