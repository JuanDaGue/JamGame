# Documentación Técnica: Duck Hunter Minigame

Este documento detalla los scripts específicos del minijuego "Duck Hunter" (Tiro al Pato con Engaño).

## Lógica Principal (Core Logic)

### 1. DuckHunterManager.cs

* **Ruta:** `Assets/Scripts/MiniGames/DuckHunter/DuckHunterManager.cs`
* **Tipo:** MonoBehaviour.
* **Ubicación en Escena:** En un GameObject manager (ej. `[DuckHunterManager]`).
* **Propósito:**
  * Orquesta el flujo del juego (3 oleadas).
  * **Lógica de Engaño:** En cada oleada, decide aleatoriamente qué tipo (`TargetType`) es el Real y cuál es la Trampa.
  * Controla la condición de Victoria (eliminar reales) y Derrota (eliminar demasiados inocentes/trampas).
* **Configuración:**
  * `Total Waves`: Número de rondas.
  * `Targets Per Color`: Cantidad de enemigos de CADA tipo (Real, Decoy, Neutral) por oleada.
    * Ejemplo: Si es 5, spawnearán 5 Reales, 5 Trampas y 5 Neutrales (Total 15).
  * `Spawn Rate`: Velocidad de aparición.

### 2. DuckTarget.cs

* **Ruta:** `Assets/Scripts/MiniGames/DuckHunter/DuckTarget.cs`
* **Tipo:** MonoBehaviour.
* **Ubicación en Escena:** En los prefabs de los objetivos (Pato, Disco, etc.).
* **Propósito:**
  * Controla el movimiento del objetivo (Lineal o ZigZag).
  * Almacena su identidad (`Real`, `Decoy`, `Neutral`).
  * Al recibir un disparo, notifica al Manager (`RegisterHit`).
  * **Nota:** Ya no gestiona el color visual en tiempo de ejecución; confía en que el Prefab instanciado sea del color correcto.

---

## Generación (Spawning)

### 3. DuckSpawner.cs

* **Ruta:** `Assets/Scripts/MiniGames/DuckHunter/DuckSpawner.cs`
* **Tipo:** MonoBehaviour.
* **Ubicación en Escena:** En un objeto Spawner (puede ser hijo del Manager).
* **Propósito:**
  * Instancia los objetivos fuera de la pantalla.
  * Genera una lista exacta ("bolsa") con la cantidad requerida de cada color para garantizar el balance.
  * Asigna aleatoriamente propiedades iniciales (Velocidad, Patrón de movimiento).
* **Configuración:**
  * `Green Target Prefab`: Prefab para objetivos Verdes.
  * `Red Target Prefab`: Prefab para objetivos Rojos.
  * `Blue Target Prefab`: Prefab para objetivos Azules.
  * `Spawn X/Y`: Límites del área de aparición.

---

## Entrada y UI (Input & Feedback)

### 4. DuckHighnoonInput.cs

* **Ruta:** `Assets/Scripts/MiniGames/DuckHunter/DuckHighnoonInput.cs`
* **Tipo:** MonoBehaviour.
* **Ubicación en Escena:** Cámara Principal o Manager.
* **Propósito:**
  * Maneja el disparo usando el **New Input System**.
  * Lanza un Raycast desde el centro de la pantalla (FPS Estático).
* **Requisitos:** Necesita una referencia a una `InputAction` configurada (Click Izquierdo).

### 5. DuckHunterUI.cs

* **Ruta:** `Assets/Scripts/MiniGames/DuckHunter/DuckHunterUI.cs`
* **Tipo:** MonoBehaviour.
* **Ubicación en Escena:** En el Canvas del minijuego.
* **Propósito:**
  * Muestra la instrucción engañosa ("Dispara al Rojo") cuando el Manager lo indica.
  * Muestra iconos y feedback visual.
