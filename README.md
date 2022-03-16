
Universal Render Pipeline extends prj.

Copy folder "com.unity.render-pipelines.universal" to Packages

==========================Basic:

develop version:
Unity2021.2.14, urp12.1.5

git clone from this:
https://github.com/Unity-Technologies/Graphics.git

git checkout 12.1.5


==========================Features
1 Amd Fsr
    MainCamera check Rendering/PostProcessing
    AMD FRS 1.0, select a item.

2 Camera rendering in gamma space
    add new Camera
    check Redering/CullingMask is UI
    check Rendering/Color Space Usage to gamma

    set UIObject's layer is UI
    URP ForwardRendererData Transparent Layer Mask remove UI(otherwist will render ui twice)
    done

