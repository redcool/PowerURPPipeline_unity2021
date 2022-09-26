
Universal Render Pipeline extends prj.

Copy folder "com.unity.render-pipelines.universal" to Packages

==========================Basic:

develop version:
Unity2021.2.14, urp12.1.7

git clone from this:
https://github.com/Unity-Technologies/Graphics.git

git checkout 12.1.7

v1.0.1
==========================Features
1 Amd Fsr
    MainCamera check Rendering/PostProcessing
    AMD FRS 1.0, select a item.


2 Camera rendering in gamma space for linear space project.
    1 Edit/ProjectSettings/Player/Rendering/ Color Space*
        select Linear
    
    2 add new Camera
        check Redering/CullingMask is UI
        check Rendering/Color Space Usage to gamma

    3 set UIObject's layer is UI
    4 URP ForwardRendererData Transparent Layer Mask remove UI(otherwist will render ui twice)

FAQ:
    1 if gamma ui not work in urp?
        1 copy Shaders\UI\UI-Default.shader to project Assets folder
        2 relanuch project.

    done
    2 amdfsr device types
        1 anroid vulkan
        2 pc 
        3 metal : not check yet
    3 gamma ui
        1 android vulkan,pc
        2 gles3, blit one more time than vulkan

