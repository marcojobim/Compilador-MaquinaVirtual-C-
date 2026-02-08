# 🚀 Compilador e Máquina Virtual (Mini-PHP)

> Um projeto acadêmico de construção de um compilador completo (Léxico, Sintático e Gerador de Código) e uma Máquina Virtual baseada em pilha.

![Status](https://img.shields.io/badge/Status-Concluído-brightgreen)
![Language](https://img.shields.io/badge/Linguagem-C%23-blue)
![Platform](https://img.shields.io/badge/Plataforma-.NET%208.0-purple)

## 📋 Sobre o Projeto

Este projeto implementa um compilador para uma linguagem baseada na sintaxe do **PHP**, gerando código objeto para uma máquina hipotética, e uma **Máquina Virtual (VM)** capaz de interpretar e executar esse código gerado.

O sistema foi desenvolvido do zero em **C#**, sem o uso de ferramentas automáticas de geração de parsers (como ANTLR ou Yacc), utilizando a técnica de **Análise Sintática Descendente Recursiva**.

### 🏗️ Arquitetura

O fluxo de execução do projeto segue os estágios clássicos de compilação:

1.  **Análise Léxica:** Transforma o código fonte em uma sequência de Tokens.
2.  **Análise Sintática:** Verifica a gramática e a estrutura do código (Parser).
3.  **Análise Semântica:** Verifica escopos, variáveis e tipos (Tabela de Símbolos).
4.  **Geração de Código:** Traduz a árvore sintática para instruções de máquina.
5.  **Máquina Virtual:** Carrega o código objeto e executa linha a linha.

## ✨ Funcionalidades

### 1. O Compilador
Suporta as seguintes estruturas da linguagem:
- **Variáveis:** Tipagem dinâmica (focada em `float`/`double`), iniciadas com `$`.
- **Controle de Fluxo:** `if`, `else`, `while`.
- **Funções:** Suporte a múltiplos parâmetros, escopo local/global e recursão.
- **Entrada/Saída:** `echo`, `readline`, `floatval`.
- **Comentários:** Linha (`//`) e Bloco (`/* ... */`).

### 2. A Máquina Virtual (VM)
Uma CPU baseada em pilha com instruções de:
- **Aritmética:** `SOMA`, `SUBT`, `MULT`, `DIVI`.
- **Lógica:** `CMMA` (>), `CMME` (<), `CPMI` (==), etc.
- **Memória:** `ALME` (Alocar), `ARMZ` (Armazenar), `CRVL` (Carregar Valor).
- **Desvio:** `DSVI` (Incondicional), `DSVF` (Condicional Falso).
- **Procedimentos:** `CHPR` (Chamada), `RTPR` (Retorno), `PUSHER` (Empilhar Endereço).

---

## 🛠️ Tecnologias Utilizadas

- **Linguagem:** C# (.NET 8.0)
- **IDE:** Visual Studio Code
- **Conceitos:** Recursive Descent Parser, Stack-Based VM, Symbol Table.

---

## 🚀 Como Executar

### Pré-requisitos
- [.NET SDK 8.0](https://dotnet.microsoft.com/download) instalado.

### Passo a Passo

1. **Clone o repositório:**
   ```bash
   git clone [https://github.com/SEU_USUARIO/SEU_REPOSITORIO.git](https://github.com/SEU_USUARIO/SEU_REPOSITORIO.git)
   cd SEU_REPOSITORIO

2. **Compile e Gere o Código Obejto**
    ```bash
    dotnet run --project Compilador -- correto.php.txt

3. **Execute o Código Objeto na Máquina Virutal**
    ```bash
    dotnet run --project VM -- teste.txt

## 💡 Dicas Adicionais
- **O arquivo correto.php.txt deve estar acessível (na raiz ou passando o caminho completo).**
- **Se você executar a VM sem argumentos (dotnet run --project VM), ela buscará automaticamente pelo arquivo padrão teste.txt.**

