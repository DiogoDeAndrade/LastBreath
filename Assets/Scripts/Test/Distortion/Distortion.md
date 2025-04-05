# Using this distortion effect

- Create layer for Distortion Sprites
- Create an empty object below the camera that should be affected
- Add a DistortionCamera component on it
  - Setup the culling layer and the resolution
- On any renderer object that should drive the drive the distortion, set it to the distortion layer
  - Set a material that can actually draw on that layer (usually unlit shader or something similar)