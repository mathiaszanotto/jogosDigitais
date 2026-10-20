# Correspondência com a rubrica

Documento de referência: Projeto Introdução ao Unity, Insper. Este quadro descreve a implementação; a avaliação e a atribuição de pontos cabem à disciplina.

| Critério | Implementação verificável |
|---|---|
| Tutorial | Duas cenas, SpriteRenderer, Rigidbody2D, colliders/triggers, PlayerMovement, normalização diagonal, prefabs, áudio e estado estático GameController. |
| Cronômetro | Tempo decorrido visível no HUD e tempo final registrado em vitória e derrota. |
| Tempo como mecânica | Reserva inicial de sangue equivalente a 45s, bônus de 3s por frasco, derrota ao zerar e bônus de score pela reserva restante na vitória. |
| Inimigos | Dois templários com rotas de patrulha, perseguição por proximidade/linha de visão e dano por contato, com invulnerabilidade temporária. |
| Visual | Paleta coerente e sprites próprios de vampiro, templário, frasco, muralhas, lajes, sarcófagos e selo de sangue em 32×32 pixels. |
| Música e efeitos | ComingHome_8bit no menu e AceOfSpades_8bit no gameplay, efeito de coleta e efeitos distintos de dano, vitória e derrota. |
| UI | Frascos, score, reserva de sangue, cronômetro, vida e barra de stamina; fonte incorporada com glifos de vida/reserva de sangue; menu temático e telas de resultado acima dos sprites. |
| Controle | Analógico, direcional, RB para investida, A para confirmar, Start para iniciar/reiniciar e View para retornar. |
| Level design | Castelo com seis obstáculos internos, corredores, duas patrulhas e frascos distribuídos pela arena. |
| Dead ends / softlocks | Todos os frascos têm rotas verificadas; derrotas explícitas por vida/tempo; reinício e retorno ao menu sempre disponíveis. |
| WebGL | Build oficial `Succeeded`, zero erros; 27 verificações no Edge/WebGL, incluindo iframe. Relatórios em `Documentation/webgl-build.json` e `Documentation/Browser/validation.json`. |
| Página do itch.io | Capa, screenshots, descrição, controles, autoria, créditos e instruções de publicação preparados na pasta Delivery e no PressKit. |
| GitHub | Projeto e código organizados para o repositório privado solicitado pelo autor. A rubrica pede público; essa exceção é uma decisão do autor. |



