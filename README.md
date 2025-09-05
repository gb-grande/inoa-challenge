# inoa-challenge
Desafio de programação desenvolvido para o processo seletivo de estágio da Inoa Sistemas

## Como configurar o servidor SMTP a ser usado

O arquivo SmtpConfig.json é um arquivo de exemplo, ele contém os campos modificáveis como host, porta, usuário, senha, email do remetente, email do destinatário e se é para usar SSL.

## Como compilar em um executável

Execute na pasta raiz do projeto:
```console
dotnet build
```
O binário estará dentro da pasta bin/release/net9.0 

## Como usar o comando
```console
Usage:
  InoaChallenge <stock> <SellPrice> <BuyPrice> [options]

Arguments:
  <stock>      The stock to monitor
  <SellPrice>  The price which triggers sell email
  <BuyPrice>   The price which triggers buy email

Options:
  -?, -h, --help  Show help and usage information
  --version       Show version information
  --time          The time period (in seconds) to make an api call [default: 60]
  --token         The brapi token to use if necessary


```
## Informações adicionais

O projeto foi feito utilizando a API [brapi](https://brapi.dev/). Ela permite consultar as seguintes ações sem precisar de autenticação: PETR4, MGLU3, VALE3, ITUB4. Caso quiser monitorar outras ações, será necessário passar o token da api com a flag --token. Eles disponibilizam um plano gratuito. Além disso, é possível especificar o período de tempo entre cada request feita pelo programa por meio da flag --time. O programa possui uma pequena tolerância para flutuações de preço. Caso um preço tenha atingido o valor que dispare um email, mas depois volte a um valor anterior, o programa permitirá uma variação de 3% em relação ao valor de gatilho antes de considerar que mudou o estado.

