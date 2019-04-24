# API文档说明
## 1.区块查询相关API
主链地址：https://api.alchemint.io/api/mainnet

测试链地址：https://api.alchemint.io/api/testnet

私链地址：https://api.alchemint.io/api/pri

以下所有接口所有环境都通用

###  getblock
|                |                          |  remark    |
|----------------|-------------------------------|-------------------------------|
|description|Get block number       |  |
|request          |{"jsonrpc": "2.0","method": "getblockcount","params": [],"id": 1}| params is null|
|response         |{"jsonrpc":"2.0","id":1,"result":[{"blockcount":"3661001"}]}| |

###  getapplicationlog
|                |                          |  remark    |
|----------------|-------------------------------|-------------------------------|
|description|Get application log by txid      |  |
|request          |{"jsonrpc": "2.0","method": "getapplicationlog","params": [],"id": 1}| params is null|
|response         |{"jsonrpc":"2.0","id":1,"result":[{"blockcount":"3661001"}]}| |
