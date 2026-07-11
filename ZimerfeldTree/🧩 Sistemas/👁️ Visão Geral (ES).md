---
tipo: sistema
projeto: GitExtensions.ZimerfeldTree
lang: es-ES
atualizado: 2026-07-04
tags: [sistema, overview, plugin, gitextensions, winforms, gitflow]
versao: 1.0.361
---

# Visión General

> 🇧🇷 Lee esta página en portugués → [[👁️ Visão Geral]]
> 🇺🇸 Read this page in English → [[👁️ Visão Geral (EN)]]

## Qué es

Plugin para **[GitExtensions](https://gitextensions.github.io/)** (Windows) que muestra las branches del repositorio **jerárquicamente en árbol** (mostrando branches hijas) en lugar de la lista plana predeterminada, y permite usar la metodología **GitFlow** de forma visual, fácil e intuitiva. Tiene su propio icono "Árbol de la Vida" dibujado/incrustado (GDI+ / `Resources/ico.png`). Nota de proyecto: [[🌳 GitExtensions.ZimerfeldTree (ES)|GitExtensions.ZimerfeldTree]].

## Stack

| Elemento | Valor |
|---|---|
| Lenguaje | C# (.NET 9), `Nullable` + `ImplicitUsings`, `LangVersion=latest` |
| Target | `net9.0-windows` |
| Framework de UI | Windows Forms (`UseWindowsForms`) |
| Tipo de salida | `Library` (DLL cargada por GitExtensions, no un exe) |
| Ensamblado de salida | `GitExtensions.Plugins.ZimerfeldTree.dll` |
| Espacio de nombres raíz | `GitExtensions.ZimerfeldTree` |
| Modelo de plugin | MEF (`System.ComponentModel.Composition`) — ver [[🧩 Plugin MEF para GitExtensions (ES)|Plugin MEF para GitExtensions]] |
| Versión actual | **1.0.358** |
| Idiomas | Inglés / Portugués (por ventana, persistido individualmente) |
| Autor | Zimerfeld |

> **Referencias externas** (de `C:\Program Files\GitExtensions\`, `Private=false`, no copiadas): `GitExtensions.Extensibility.dll`, `GitUIPluginInterfaces.dll`, `System.ComponentModel.Composition.dll`.

## Las tres ventanas

### 1. Branch Hierarchy (`BranchHierarchyForm`)
Ventana **no modal**, singleton por sesión, se abre centrada y redimensionable, independiente de GitExtensions. Árbol en 3 secciones fijas — **LOCAL**, **REMOTES**, **TAGS** — con contadores `(N)`, la branch actual en negrita, filtro en tiempo real y botones **Pull / Push / Commit / Eliminar / GitFlow / Restore** encima del árbol. Carga asíncrona con overlay de progreso en la 1ª apertura. Detalles control a control: [[🌳 Interface ZimerfeldTree — botões e fluxos (ES)|Interfaz de ZimerfeldTree — botones y flujos]].

### 2. GitFlow (`GitFlowForm`)
Ventana modal que dirige los comandos `git flow` usando **solo git nativo** (no depende del binario `git-flow` instalado): start/publish/track/update/finish para feature, bugfix, release, hotfix y support. Permite una **jerarquía flexible** (feature hija de feature, vía *based on:*). Detalles: [[🔀 Interface GitFlow — botões e fluxos (ES)|Interfaz de GitFlow — botones y flujos]] y [[⚙️ git flow - chaves de config (CLI) (ES)|git flow - claves de config (CLI)]].

### 3. Restore (`RestoreForm`)
Central para "volver en el tiempo" con 10 pestañas, de la más segura a la más destructiva: Plan de Emergencia, Restaurar Archivo/Árbol/Tag, Cherry-Pick, **Revertir**, Reset Branch, **Nueva Branch/Tag**, **Recuperar (Reflog)**, **Descartar Locales** y **Rebase**. Cada categoría con explicación incrustada y orientaciones de trabajo en equipo. Detalles: [[⏪ Interface Restore — botões e fluxos (ES)|Interfaz de Restore — botones y flujos]].

## Jerarquía flexible de GitFlow — feature hija de feature

El GitFlow clásico no contempla una feature hija de otra feature (todas las `feature/*` derivan de `develop` y son hermanas). El **ZimerfeldTree GitFlow** permite una jerarquía flexible donde una `feature/*` puede derivar de `develop` **o de otra `feature/*`** por encima de ella (vía *based on:* en el Start). Consecuencia: el *finish feature* debe **encadenarse** hasta el nodo padre sucesivamente, reaplicando *finish feature* hasta llegar a `develop`.

## Localización (Inglés / Portugués)

Cada ventana elige su idioma de forma **independiente** y lo recuerda. La ventana principal usa `I18n.SetLanguage` global (persistido en `ZimerfeldTree.language.json`); GitFlow y Restore tienen su propio selector persistido en sus respectivos archivos de settings.

## Relacionado

- [[🌳 GitExtensions.ZimerfeldTree (ES)|GitExtensions.ZimerfeldTree]]
- [[🏷️ Versionamento (ES)|Versionado]]
- [[📘 README — Instalação, Uso e Build (ES)|README — Instalación, Uso y Build]]
- [[🌳 Interface ZimerfeldTree — botões e fluxos (ES)|Interfaz de ZimerfeldTree — botones y flujos]]
- [[🔀 Interface GitFlow — botões e fluxos (ES)|Interfaz de GitFlow — botones y flujos]]
- [[⏪ Interface Restore — botões e fluxos (ES)|Interfaz de Restore — botones y flujos]]
- [[🔑 Fatos-Chave (ES)|Datos Clave]]
