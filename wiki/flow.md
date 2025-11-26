# Flujo de la aplicación MySaaSAgent

Este documento describe el **flujo completo** que sigue la aplicación desde el registro del negocio hasta la fidelización del cliente, tal como se explicó en la solicitud del usuario.

---

## Fase 1 – Onboarding del Negocio (Configuración del Cerebro)
1. **Registro y prueba** – El dueño del negocio se registra en la web y se le activa un trial de 14 días.
2. **Inyección de contexto (Orquestador)** – El dueño completa un formulario con información clave (qué vende, horarios, precios, política de cancelación).
   - El sistema genera la **Base de Conocimiento** (para que el Bot de Recepción responda) y las **Reglas de Negocio** (para que el Bot Transaccional conozca la disponibilidad).

---

## Fase 2 – Entrada del Usuario Final (Cliente del Cliente)
### A. Vía App Móvil (Marketplace/Directorio)
- El usuario abre la app, visualiza una lista de negocios y selecciona uno.
- Puede ver el calendario o catálogo y pulsar **Agendar** o **Comprar**.
- El sistema guarda directamente en la base de datos y envía una notificación push al dueño.

### B. Vía Chat (WhatsApp / Instagram / Meta)
- El usuario inicia conversación con el número del negocio.
- Aquí entran en juego los bots.

---

## Fase 3 – Flujo Conversacional (Interacción de Bots)
1. **Bot de Recepción y Contexto** – Analiza el mensaje, consulta la Base de Conocimiento y responde preguntas genéricas. Detecta intenciones de reserva o compra y delega al siguiente bot.
2. **Bot Transaccional / Setter** – Gestiona la reserva o la venta:
   - **Servicio**: consulta la agenda real, propone horarios y bloquea el espacio.
   - **Venta**: captura datos del pedido, califica al cliente y envía link de pago o notifica a un asesor.
   - Crea un registro en el Dashboard con estado **Pendiente**.
3. **Bot Notificador** – Envía confirmación al cliente vía WhatsApp: *"Tu cita/pedido ha sido confirmado para el día X"*.

---

## Fase 4 – Ejecución y Gestión (Dashboard)
- El dueño recibe una alerta en la app/web.
- El servicio se realiza o el producto se entrega.
- El dueño marca la transacción como **Completado** en el Dashboard, lo que dispara la fase de post‑venta.

---

## Fase 5 – Post‑Venta y Fidelización
1. **Bot de Reputación (Feedback)** – Después de un tiempo, pregunta al cliente por su experiencia y solicita reseña o registra quejas.
2. **Bot de Reactivación (Vendedor Silencioso)** – Detecta inactividad (30 días, 1 año, etc.) y envía un mensaje proactivo para volver a agendar.

---

## Resumen de componentes obligatorios
- **Dashboard/App (Negocios)** – Configuración y gestión de estados.
- **App Móvil (Usuarios)** – Marketplace sin chat.
- **Bot de Recepción** – IA generativa + RAG.
- **Bot Transaccional** – Lógica rígida + DB.
- **Bot Notificador / Reputación** – Eventos y confirmaciones.
- **Bot Reactivación** – Cron jobs / tiempo.

Este flujo garantiza que cualquier tipo de negocio (abogado, barbería, tienda) pueda operar con la misma arquitectura modular y extensible.
