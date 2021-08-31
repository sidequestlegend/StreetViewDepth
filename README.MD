#### Getting Started

Clone the repo ( or use the unity package in the releases) into the `Assets` folder in your unity project.

Add the `Sphere.prefab` to your Scene. You can change the `Panoid` property on the `PhotoSphere` component on the `Sphere.prefab` to any you like.

To get the `Panoid` from a street view page url: 

```
https://www.google.nl/maps/@52.239981,6.851849,3a,90y,324.71h,64.65t/data=!3m5!1e1!3m3!1sFEpIJhSgOzoAAAQJOQCL3w!2e0!3e11
```

After the `!1s` and before the `!2e` is the Panorama ID, so here is it:

`FEpIJhSgOzoAAAQJOQCL3w`

