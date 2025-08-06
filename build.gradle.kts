plugins {
    id("java")
    id("application")
    id("org.openjfx.javafxplugin") version "0.1.0"
}

repositories {
    mavenCentral()
}

javafx {
    version = "21"
    modules = listOf("javafx.controls", "javafx.fxml")
}

application {
    mainClass.set("com.vortex.Vortex")
}

tasks.withType<JavaCompile> {
    options.encoding = "UTF-8"
}

val copyJavafxDlls by tasks.registering(Copy::class) {
    val javafxModules = listOf("javafx-controls", "javafx-fxml")
    val os = "win"  // 必要に応じて "linux" や "mac" に変更してください

    from(javafxModules.map { module ->
        zipTree(configurations.runtimeClasspath.get().find {
            it.name.contains("$module-21") && it.name.contains(os)
        } ?: throw GradleException("JavaFX module $module not found"))
    })
    include("**/*.dll")
    into("$buildDir/install/Vortex/lib")
}

tasks.register<Zip>("distZipWithDll") {
    dependsOn(copyJavafxDlls)
    dependsOn("installDist")

    from("$buildDir/install/Vortex")
    archiveFileName.set("Vortex.zip")
    destinationDirectory.set(file("$buildDir/distributions"))
}
