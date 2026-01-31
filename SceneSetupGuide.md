# Guía de Configuración Manual de Escenas: Carnival y Duck Hunter

Sigue estos pasos para configurar las escenas en Unity utilizando primitivas (Cubos, Esferas, etc.) como placeholders.

---

## 1. Escena: Carnival (Hub de Selección)

Esta es la escena donde el jugador camina y elige qué jugar.

1. **Crear la Escena**:
   * File -> New Scene -> Basic (Built-in).
   * Guardar como: `Carnival` (en `Assets/Scenes`).

2. **El Suelo (Floor)**:
   * Resetear Transform (Pos: 0,0,0). Escalalo (10, 1, 10) para tener espacio.

3. **Cámara (Cinemachine 3.x)**:
   * Selecciona la **Main Camera**.
   * Add Component -> **`CinemachineBrain`**.
   * (Opcional) En el menú superior: Cinemachine -> Create Cinemachine Camera (esto crea un objeto CM vcam).
   * Si lo haces manual: Crea un GameObject `CM_HubCam`, agrégale el componente **`CinemachineCamera`**.
   * Posición de `CM_HubCam`: `(0, 1, -5)` (o donde quieras la vista).

4. **El Puesto (Stand) de Duck Hunter**:
   * Crear un **Cubo**: GameObject -> 3D Object -> Cube.
   * Nombre: `Stand_DuckHunter`.
   * Posición: `(0, 1, 5)` (Para que quede frente a la cámara).
   * (Opcional) Material: Crea un material Naranja y asígnalo para diferenciarlo.

5. **Configurar la Lógica del Stand**:
   * Selecciona `Stand_DuckHunter`.
   * Add Component -> **`MinigameStand`**.
   * **Configuración del Script**:
     * **`Minigame Scene File`**: Arrastra directamente el archivo de escena `Duck_Hunter` desde tu carpeta de `Scenes` a este campo.
     * `Minigame Scene Name`: Verás que se rellena solo con "Duck_Hunter".
     * `Associated Mask`: (Déjalo vacío por ahora).
     * `Completed Indicator`: (Déjalo vacío por ahora).

6. **Configurar Input (Hub)**:
   * Necesitamos un script que detecte el click usando el New Input System.
   * Crea un GameObject vacío: `InputManager`.
   * Add Component -> **`HubInteractionInput`** (Verifica que tengas este script creado).
   * Asignale la referencia a tu Acción de Click (Input Action Reference).
   * *Verificación:* Dale Play y haz click en el Cubo.

---

## 2. Escena: Duck_Hunter (El Minijuego)

Esta es la escena del fps estático.

1. **Crear la Escena**:
   * File -> New Scene -> Basic.
   * Guardar como: `Duck_Hunter` (en `Assets/Scenes`).

2. **Configurar Build Settings** (¡Importante!):
   * File -> Build Settings.
   * Arrastra ambas escenas (`Carnival` y `Duck_Hunter`) a la lista "Scenes In Build".

3. **Cámara (Cinemachine 3.x)**:
   * **Main Camera**:
     * Add Component -> **`CinemachineBrain`**.
     * Add Component -> **`DuckHighnoonInput`**.
     * *Nota:* `DuckHighnoonInput` sigue yendo en la Main Camera porque necesitamos el componente `Camera` para los Raycasts. Cinemachine moverá esta cámara, pero el script vive aquí.
   * **Virtual Camera**:
     * Crea un GameObject `CM_GameCam`.
     * Add Component -> **`CinemachineCamera`**.
     * Posición: `(0, 1, -10)` mirando al `(0, 0, 0)`.

4. **Game Manager (El Cerebro)**:
   * Crear GameObject Vacío: `GameManager`.
   * Add Component -> **`MinigameController`**.
     * **`Hub Scene File`**: Arrastra el archivo de escena `Carnival`.
     * `Hub Scene Name`: Se rellenará solo.
     * `Reward Mask`: (Déjalo vacío hasta que crees el Item).
   * Add Component -> **`DuckHunterManager`**.
     * `Targets Per Wave`: 5 (para probar rápido).

5. **Spawner (Generador de Patos)**:
   * Crear GameObject Vacío: `Spawner`.
   * Add Component -> **`DuckSpawner`**.
   * **Configuración**:
     * `Target Prefab`: (Ver Paso 6).
     * `Spawn X`: 10 (Que tan lejos a los lados nacen).
   * **Enlazar**:
     * Selecciona `GameManager`.
     * Arrastra el objeto `Spawner` al campo `Spawner` del `DuckHunterManager`.

6. **El Objetivo (Prefab del Pato)**:
   * Crear una **Esfera** en la escena. Nombre: `Duck_Target_Base`.
   * Add Component -> **`DuckTarget`**.
   * Arrastra esta Esfera desde la Jerarquía a tu carpeta `Assets/Prefabs` para convertirla en Prefab.
   * Borra la esfera de la escena.
   * **Asignar**: Selecciona el objeto `Spawner` y arrastra este nuevo Prefab al campo `Target Prefab`.

7. **La UI (Interfaz)**:
   * GameObject -> UI -> Text - TextMeshPro. (Importa TMP Essentials si te pide).
   * Nombre: `InstructionText`. Posiciónalo arriba al centro.
   * **Enlazar**:
     * Crear GameObject UI Vacío: `UIManager`.
     * Add Component -> **`DuckHunterUI`**.
     * Arrastra `InstructionText` al campo `Instruction Text`.
     * Selecciona `GameManager`. Arrastra `UIManager` al campo `Ui` del `DuckHunterManager`.

Listo. Con esto tienes la estructura básica funcional.
