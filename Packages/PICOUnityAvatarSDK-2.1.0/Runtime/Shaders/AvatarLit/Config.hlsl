#ifndef AVATAR_CONFIGURE
#define AVATAR_CONFIGURE
// 当此变量为1时，avatar shader feature 失效，走下面配置的关键字
// 当此变量为0时，avatar shader 退化为正常shader与SVN仓库保持一致
#define REQUIRE_BAKE_AVATAR_KEYWORDS 1

#if REQUIRE_BAKE_AVATAR_KEYWORDS

// Universal Keywords
    #define _NORMALMAP
    //#define _ACES 1

// Custom Keywords
    #if  defined(AVATAR_LIT)
        // #define _MATCAP
        // #define _MATCAP_FIX_EDGE_FLAW
    #elif defined(AVATAR_SKIN)
        #define _RIM_LIGHT
        #define _RIMMASK_VIEW
        #define _DUAL_SPECULAR
    #elif defined(AVATAR_HAIR)

    #elif defined(AVATAR_EYE)
        #define _REFMODE_POM
        #define _SHARP_HIGHTLIGHT
    #endif
#endif

#endif 
