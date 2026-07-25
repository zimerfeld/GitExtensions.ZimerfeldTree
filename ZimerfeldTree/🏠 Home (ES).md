---
tipo: moc
projeto: GitExtensions.ZimerfeldTree
lang: es-ES
atualizado: 2026-07-04
versao: 1.0.361
tags: [moc, home, zimerfeld, tree, gitextensions, gitflow]
---

# 🏠 GitExtensions.ZimerfeldTree — Bóveda de Neuronas

> 🇧🇷 Lee esta página en portugués → [[🏠 Home (PT)|🏠 Home]]
> 🇺🇸 Read this page in English → [[🏠 Home (EN)]]

> [!abstract] 🧠 Qué es esta bóveda
> Memoria persistente de Claude para el proyecto **GitExtensions.ZimerfeldTree** — plugin para GitExtensions que muestra las branches en **árbol jerárquico** y ofrece **GitFlow visual** en git puro. La bóveda se lee al inicio de cada sesión y se actualiza siempre que algo relevante cambia.

![[ScreenshotBranchHierarchy.png]]

## ⚡ Resumen ejecutivo

- **Qué es:** extensión (plugin MEF) para **GitExtensions** que sustituye la lista plana de branches por un **árbol jerárquico** (LOCAL / REMOTES / TAGS) y expone el flujo **GitFlow** con clics, sin depender del binario `git-flow` instalado. Tres ventanas dedicadas: **Branch Hierarchy** (árbol no modal), **GitFlow** (start/publish/track/update/finish) y **Restore** (10 pestañas de "volver atrás en el tiempo"). Icono propio "Árbol de la Vida" (GDI+ / `Resources/ico.png`).
- **Problema que resuelve:** navegar branches en una lista plana y conducir GitFlow por línea de comandos es lento y propenso a errores. El plugin da **contexto visual de parentesco** entre branches y transforma start/publish/track/update/finish en botones — con registro de cada comando git ejecutado.
- **Diferenciales:** **GitFlow flexible** (feature hija de feature vía *based on:*, con *finish* en cascada hasta `develop`); ventana **no modal** y asíncrona (se abre al instante, con overlay de progreso); **contador de Commit en vivo** (FileSystemWatcher); **push protegido** contra divergencia (ofrece pull --rebase + push automático); **Modo Developer** que protege `main`/`master`/`develop`; ventana **Restore** con 10 niveles de recuperación; **i18n** (Inglés / Portugués, por ventana); banner de patrocinio (GitHub Sponsors + Ko-fi).
- **Stack:** C# / WinForms `Library`, destino **net9.0-windows**, ensamblado `GitExtensions.Plugins.ZimerfeldTree.dll`, empaquetado como **nupkg**; build y versionado automatizados vía `build.ps1`.
- **Estado actual:** versión **`1.0.361`** — funcional, con las tres ventanas en producción.
- **Público objetivo:** desarrolladores y equipos que usan GitExtensions en Windows y adoptan (o quieren adoptar) GitFlow con claridad visual de la jerarquía de branches.
- **Ángulo de negocio/portafolio:** producto **open source** bajo el owner `zimerfeld`, que refuerza la autoridad técnica y sirve de vitrina para la adopción (clones/descargas de NuGet) y la captación de patrocinio. Se integra con su hermano **GitExtensions.ZimerfeldCommitMsg**.

## 🧭 Navegación por prioridad

### 1️⃣ 🔑 Impacto — Archivos Clave
> Archivos que, al ser modificados, tienen gran impacto en el sistema.
- [[🌳 ZimerfeldTreePlugin (ES)|🌳 ZimerfeldTreePlugin]] — punto de entrada MEF (`IGitPlugin`)
- [[🪟 BranchHierarchyForm (ES)|🪟 BranchHierarchyForm]] — la ventana principal (árbol, botones, contador en vivo)
- [[⚙️ BranchHierarchyService (ES)|⚙️ BranchHierarchyService]] — ejecutor git + ensamblado de la jerarquía
- [[🔀 GitFlowForm (ES)|🔀 GitFlowForm]] — la ventana GitFlow (start/finish, based on)
- [[⏪ RestoreForm (ES)|⏪ RestoreForm]] — la ventana Restore (10 pestañas)
- [[🔧 build.ps1 (ES)|🔧 build.ps1]] — build + versionado + deploy
- [[🌿 BranchNode (ES)|🌿 BranchNode]] — modelos `BranchInfo` + `BranchType`
- [[🎨 NodeIcons (ES)|🎨 NodeIcons]] — iconos 16×16 del árbol (GDI+ + PNGs incrustados)
- [[🖼️ PluginIcon (ES)|🖼️ PluginIcon]] — icono del plugin/ventana (`Resources/ico.png`)

### 2️⃣ 🧩 Reutilización — Sistemas
> Subsistemas reutilizados por varias partes del proyecto.
- [[👁️ Visão Geral (ES)|Visión General]] — qué hace el plugin, stack, las tres ventanas, versión actual
- [[🏗️ Arquitetura (ES)|Arquitectura]] — las clases (Plugin → Forms → Service), threading, i18n, desacoplamiento
- [[🏷️ Versionamento (ES)|Versionado]] — ciclo `build.ps1` / nuspec / csproj / sellado de docs
- [[📦 Dependências (ES)|Dependencias]] — resumen de sistema (host + `git` + build)
- [[📦 Dependências do ZimerfeldTree (ES)|Dependencias de ZimerfeldTree]] — versión detallada, paso a paso, con descargas

### 3️⃣ 🔀 Uso — Flujos
> Flujos de uso paso a paso.
- [[🌲 Abrir e navegar a árvore (ES)|Abrir y navegar el árbol]] — apertura asíncrona, overlay, navegación de la jerarquía
- [[🔀 GitFlow (Start a Finish) (ES)|GitFlow (Start a Finish)]] — el ciclo GitFlow en git puro
- [[⏪ Restore (voltar no tempo) (ES)|Restore (volver atrás en el tiempo)]] — las 10 pestañas de recuperación

## 🚀 Operación
- [[💻 Ambiente Local (Dev) (ES)|Entorno Local (Dev)]] — `.\build.ps1` (Admin) · deploy rápido: `.\tools\update-dll.ps1`
- [[🚀 Deploy em Produção (Prod) (ES)|Deploy en Producción (Prod)]] — `.\build.ps1` → publicar `.nupkg` (nuget.org + GitHub release)

## ⚖️ Decisiones
- [[🌳 Árvore hierárquica vs lista plana (ES)|Árbol jerárquico vs lista plana]] — por qué árbol en vez de la lista plana estándar
- [[🔀 GitFlow em git puro (ES)|GitFlow en git puro]] — sin dependencia del binario `git-flow`
- [[🌿 GitFlow flexível — feature filha de feature (ES)|GitFlow flexible — feature hija de feature]] — jerarquía más allá del GitFlow clásico
- [[🪟 Janela não-modal singleton (ES)|Ventana no modal singleton]] — ventana persistente + working dir independiente
- [[🛡️ Modo Developer protege main-develop (ES)|Modo Developer protege main-develop]] — protección contra eliminación accidental
- [[⏪ Restore — central de voltar no tempo (ES)|Restore — central de volver atrás en el tiempo]] — 10 pestañas ordenadas por riesgo

## 📚 Conocimiento
- [[🌳 Interface ZimerfeldTree — botões e fluxos (ES)|Interfaz ZimerfeldTree — botones y flujos]] — el árbol no modal (Pull/Push/Commit/Eliminar/GitFlow/Restore, filtro, overlay, contador en vivo)
- [[🔀 Interface GitFlow — botões e fluxos (ES)|Interfaz GitFlow — botones y flujos]] — start/publish/track/update/finish y la jerarquía *based on:*
- [[⏪ Interface Restore — botões e fluxos (ES)|Interfaz Restore — botones y flujos]] — las 10 pestañas de "volver atrás en el tiempo", de la más segura a la más destructiva
- [[🧩 Plugin MEF para GitExtensions (ES)|Plugin MEF para GitExtensions]] — el modelo MEF de plugin (`IGitPlugin`)
- [[⚙️ git flow - chaves de config (CLI) (ES)|git flow — claves de config (CLI)]] — claves `gitflow.*` esperadas por la CLI vs. las escritas por GitExtensions
- [[🌿 Hierarquia de branches — branches no mesmo commit (ES)|Jerarquía de branches — branches en el mismo commit]] — por qué dos branches en el mismo commit no forman padre-hijo, y la solución (commit vacío en el Start)
- [[📘 README — Instalação, Uso e Build (ES)|README — Instalación, Uso y Build]] — espejo del README

## 💼 Negocio
- [[🌳 GitExtensions.ZimerfeldTree (ES)|🌳 GitExtensions.ZimerfeldTree]] — nota madre del proyecto (espeja el README: objetivo, estructura, funcionalidades, limitaciones, financiación)

## 🧭 Meta
- [[🧭 Como usar este cofre (ES)|Cómo usar esta bóveda]] — protocolo de lectura/escritura de Claude
- [[🔑 Fatos-Chave (ES)|Datos Clave]] — verdades siempre útiles (rutas, nombres, convenciones)
- [[📥 Inbox (ES)|📥 Inbox]] — captura rápida
- [[👤 Renato (ES)|👤 Renato]] — contexto y preferencias
- [[🧰 RTK (ES)|🧰 RTK]] — proxy CLI que ahorra tokens

## 📌 Retomada
- [[📌 Backlog (ES)|Backlog]] — **empieza aquí** al retomar el proyecto en otra sesión
