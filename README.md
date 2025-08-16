# Simple_Outline
A simple outline render feature made for Unity 6.0 URP. The code is mostly split between 4 files:
- **OutlineRenderFeature** -- Containing the two render passes and adding them to the render pipeline. This is also where we define our variables to be used in the two passes.
- **DrawObjectsPass** -- Here we define the settings for the objects we want to draw. We then override the material for the objects we want to draw and then send the command to the context. We also clear the screen beforehand, so we don't have leftover information from the previous rendering frame.
- **DrawOutlinePass** -- This takes the texture information from the previous pass and uses it to draw the outlines. We then add this texture to the original one rendered on the screen.

- **outlineShader** -- Here is the code that executes on the GPU. We simply take the original image, blur it, subtract the original one. This way the only thing that is left is is the outline. This outline is then added to the screen texture.
