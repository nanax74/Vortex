cmake_minimum_required(VERSION 3.19)
project(Vortex LANGUAGES CXX)

find_package(Qt6 6.5 REQUIRED COMPONENTS Core Widgets LinguistTools)

qt_standard_project_setup()

qt_add_executable(Vortex
    WIN32 MACOSX_BUNDLE
    main.cpp
    vortex.cpp
    vortex.h
    vortex.ui
)

qt_add_translations(
    TARGETS Vortex
    TS_FILES Vortex_ja_JP.ts
)

target_link_libraries(Vortex
    PRIVATE
        Qt::Core
        Qt::Widgets
)

include(GNUInstallDirs)

install(TARGETS Vortex
    BUNDLE  DESTINATION .
    RUNTIME DESTINATION ${CMAKE_INSTALL_BINDIR}
    LIBRARY DESTINATION ${CMAKE_INSTALL_LIBDIR}
)

qt_generate_deploy_app_script(
    TARGET Vortex
    OUTPUT_SCRIPT deploy_script
    NO_UNSUPPORTED_PLATFORM_ERROR
)
install(SCRIPT ${deploy_script})
