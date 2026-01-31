# Documentación del Sistema de Eventos y Progresión (Mega Man Style)

Este documento detalla la arquitectura, ubicación y propósito de cada script implementado en el proyecto tras la refactorización para la lógica de progresión tipo "Mega Man" (Minijuego -> Recompensa -> Ventaja en otro Minijuego).

## Arquitectura y Flujo de Escenas

Para garantizar que el `InventorySystem` (Singleton) funcione correctamente y no se duplique ni se pierda:

### 1. Secuencia de Escenas

1. **Menu** (Escena Inicial):
   * **Contenido:** UI de Ingreso.
   * **Singletons:** Aquí **DEBE** colocarse el GameObject `InventorySystem`. Al iniciar el juego, este objeto se marca como `DontDestroyOnLoad`.

2. **Carnival** (Hub de Selección):
   * **Contenido:** Stands de Minijuegos (`MinigameStand`).
   * **Acción:** Al cargar esta escena, el `InventorySystem` que viene del Menu persiste.

3. **Minigames** (Ej: `Duck_Hunter`):
   * **Contenido:** Lógica del juego (`DuckHunterManager`, `MinigameController`).
   * **Acción:** Al ganar/perder, regresan a la escena `Carnival`.

### 2. Regla de Oro para el InventorySystem

* **En Producción (Build):** El prefab `InventorySystem` solo debe estar en la escena **Menu**.
* **En Desarrollo (Editor):** Si quieres probar directamente la escena `Duck_Hunter` sin pasar por el Menu, debes arrastrar temporalmente el prefab `InventorySystem` a la escena.
  * *Nota:* El script está protegido para autodestruirse si detecta duplicados, así que si por error lo dejas en todas las escenas, no romperá el juego, pero es mejor mantenerlo limpio en la inicial.

---

## Núcleo (Core) - Definiciones y Eventos

### 1. GameEvents.cs

* **Ruta:** `Assets/Scripts/Core/GameEvents.cs`
* **Tipo:** Clase Estática (No hereda de MonoBehaviour).
* **Ubicación en Escena:** NINGUNA.
* **Propósito:** La "Biblia de Eventos". Define los canales de comunicación (`Action`) que conectan el inventario con los sistemas de juego sin acoplarlos.
* **Uso:**
  * Escuchar: `GameEvents.OnCollectibleStateChanged += MiFuncion;`
  * Notificar: `GameEvents.OnCollectibleStateChanged?.Invoke(item, true);`

### 2. CollectibleData.cs

* **Ruta:** `Assets/Scripts/Core/CollectibleData.cs`
* **Tipo:** ScriptableObject.
* **Ubicación en Escena:** NINGUNA. Son archivos de datos.
* **Propósito:** Define qué ES una máscara/recompensa (ID, Nombre, Icono).
* **Creación:** Click derecho en Project -> `Create` -> `GameJam` -> `Collectible Data`.

### 3. GameConstants.cs

* **Ruta:** `Assets/Scripts/Core/GameConstants.cs`
* **Tipo:** Clase Estática.
* **Propósito:** Centraliza constantes como Tags (`Player`) y nombres de escenas clave para evitar errores de escritura ("magic strings").

---

## Sistemas (Systems) - Lógica Global

### 4. InventorySystem.cs

* **Ruta:** `Assets/Scripts/Systems/InventorySystem.cs`
* **Tipo:** MonoBehaviour (Singleton Persistente).
* **Ubicación en Escena:** En un GameObject vacío llamado `InventorySystem` (o similar), SÓLO EN LA PRIMERA ESCENA (Hub o Menú).
* **Propósito:**
  * Almacena qué máscaras/items ha ganado el jugador.
  * Sobrevive al cambio de escenas (`DontDestroyOnLoad`).
  * Al recibir un item (ganar minijuego), dispara el evento global en `GameEvents`.
  * Usa patrón **Lazy Singleton**: Se puede llamar desde cualquier lado y si no está cacheado, lo busca.

---

## Interacciones (Interactions) - Hub & Selección

### 5. MinigameStand.cs (NUEVO)

* **Ruta:** `Assets/Scripts/Interactions/MinigameStand.cs`
* **Tipo:** MonoBehaviour.
* **Ubicación en Escena:** En los "Puestos" o Portales del Hub.
* **Propósito:**
  * Controla la entrada a un minijuego específico (`Minigame Scene Name`).
  * Muestra visualmente si ya completaste ese nivel (verifica si tienes la `Associated Mask` en el inventario).
* **Configuración:** Asignar el nombre de la escena destino y (opcionalmente) la máscara que se gana ahí para mostrar el indicador de "Completado".

---

## Minijuegos (MiniGames) - Lógica de Nivel

### 6. MinigameController.cs (NUEVO)

* **Ruta:** `Assets/Scripts/MiniGames/MinigameController.cs`
* **Tipo:** MonoBehaviour.
* **Ubicación en Escena:** En un objeto controlador dentro de la escena de CADA Minijuego.
* **Propósito:** Gestiona la condición de Victoria/Derrota.
* **Uso:**
  * Cuando el jugador gana, llama a `WinGame()`.
  * **Automáticamente** entrega la máscara configurada (`Reward Mask`) al inventario y regresa al Hub.

### 7. BuffDebuffManager.cs

* **Ruta:** `Assets/Scripts/MiniGames/BuffDebuffManager.cs`
* **Tipo:** MonoBehaviour.
* **Ubicación en Escena:** En el Player o GameManager del Minijuego.
* **Propósito:** Aplica la lógica "Mega Man". Verifica si tienes máscaras ganadas en **otros** niveles para darte ventajas en el actual.
* **Cíclo de Vida:**
  * `Start()`: Pregunta "¿Qué tengo?" para aplicar estados iniciales.
  * `OnEnable/Disable`: Se suscribe a eventos por si algo cambia en tiempo real (menos común en minijuegos aislados, pero útil como patrón).
