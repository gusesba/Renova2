Quero desenvolver um sistema completo para uma rede de brechós. Os brechós na rede funcionam da seguinte maneira:

Fornecimento de peças:
1 - Maioria das peças em consignação.
2 - Podem haver peças fixas na loja, que estarão sempre ou quase sempre disponíveis.
3 - Podem haver peças que  vem em lote e duram até acabar aquela quantia.

Pagamento aos fornecedores:
1 - Peças consignadas são pagas ao fornecedor apenas após serem vendidas.
	1.1 - Pagamento pode ser em dinheiro
	1.2 - Pagamento pode ser em crédito para a loja - Porcentagem maior sobre o produto
	1.3 - Mistura entre os dois
2 - Peças fixas e em lote são pagas ao fornecedor antes de entrar na loja.

Pagamento para a loja:
1 - Pagamento em dinheiro (dinheiro, cartão, pix, etc)
2 - Pagamento utilizando o crédito que já tem na loja
3 - Mistura entre os dois

Comportamento das Peças:
1 - As peças consignadas só devem ficar exibidas na loja por certa quantia de tempo
2 - Após certa quantidade de meses, as peças começam a receber desconto. (os 3 últimos meses, sendo um desconto progressivo)
3 - Ao acabar o tempo na loja, a peça deve ser doada ou devolvida. (Cadastro por fornecedor - padrão devolver.) (Peças específicas também podem ser registradas diferente do padrão do fornecedor).

Cadastro de peças: 
- Id 
- Preço
- Nome Produto (Referencia a tabela separada)
- Marca (Referencia a tabela separada)
- Tamanho (Referencia a tabela separada)
- Cor (Referencia a tabela separada)
- Fornecedor (Referencia a tabela separada)
- Descrição
- Data Entrada

Registro de vendas: Sempre que seja feita uma venda, registrar a compra toda em um grupo para poder saber quem comprou quando e quais peças

Registro de pagamento: Registrar pagamentos feitos para a loja e da loja para os fornecedores.
Pense na melhor forma implementar isso integrando com o sistema de crédito na loja

Registro de usuário: Usuários que vão acessar o sistema - pode ser o dono da loja, funcionários ou clientes que vão acessar para ver pendencias, saldo

Registro de cliente/fornecedor: Podem ser atribuidos a um usuário do sistema

Registro de loja: nome, endereço, etc.


Fluxo básico do sistema:
- Dono de loja:
	- Cria conta de usuário
	- Registra loja
	- Registra clientes/fornecedores
	- Cria os registros de marca/tamanho/cor/nomeproduto...
	- Cria as configurações de
		- % de cosignação normal
		- % de consignação com pagamento em créditos
		- Tempo máximo na loja
		- Desconto por tempo na loja
	 - Cria as formas de pagamento (pense em uma forma de calcular o caixa com base na forma de pagamento incluindo a taxa atual da forma de pagamento)
	 - Cria o registro das peças
	 - Cria venda
	 - Registra pagamento
	 - Ve dashboard de vendas e caixa
- Cliente:
	- Ve as lojas em que é cliente
	- Ve as peças que tem na loja (vendidas e atuais)
	- Vê situação financeira (Crédito, Pendencias, Saldo...)


Além disso:

- Deve poder imprimir etiquetas para as peças com código de barras
- Imprimir recibo para os clientes
(Essas integrações devem ser simples para o dono da loja)

O que mais:
- Exportar com filtro para o excel/pdf
- Página de fechamento do cliente (acessível para o dono da loja)
	- Mostra compras e vendas do cliente
	- Pagamentos
	- Total comprado vendido
	- etc
  - Forma fácil de copiar formatado da tabela para enviar ao cliente pelo whats ou outro. (talvez um botão para isso).


O sistema vai ser dividido nos módulos
Backend em .NET - Responsável por todas as operações
FRONTEND WEB REACT - Responsável pela visualização e interação com os usuário 
MOBILE REACT NATIVE - Algumas vizualizações para donos de loja e clientes


Essa é a ideia inicial, posso ter esquecido algo. Aceito sugestões e talvez alterações. Quero algo profissional, seguro e completo