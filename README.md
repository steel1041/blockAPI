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

###  getnotify
|                |                          |  remark    |
|----------------|-------------------------------|-------------------------------|
|description|Get application log by txid      |  |
|request          |{"jsonrpc": "2.0","method": "getnotify","params": ["f9f55031239475cff453ca3d8a2b4a61d6ad6cfbbbeae7770f10e3e357528d0c"],"id": 1}| params is txid|
|response         |{"jsonrpc":"2.0","id":1,"result":[{"txid":"0xf9f55031239475cff453ca3d8a2b4a61d6ad6cfbbbeae7770f10e3e357528d0c","executions":[{"trigger":"Application","contract":"0x4b5acd30ba7ec77199561afa0bbd49b5e94517da","vmstate":"HALT, BREAK","gas_consumed":"0","stack":[{"type":"Integer","value":"1"}],"notifications":[]}],"blockindex":3639643}]}| |

###  getTxByTxid
|                |                          |  remark    |
|----------------|-------------------------------|-------------------------------|
|description|Get transaction log by txid      |  |
|request          |{"jsonrpc": "2.0","method": "getTxByTxid","params": ["f9f55031239475cff453ca3d8a2b4a61d6ad6cfbbbeae7770f10e3e357528d0c"],"id": 1}| params is txid|
|response         |{"jsonrpc":"2.0","id":1,"result":[{"txid":"0xf9f55031239475cff453ca3d8a2b4a61d6ad6cfbbbeae7770f10e3e357528d0c","size":281,"type":"InvocationTransaction","version":1,"attributes":[{"usage":"Remark15","data":"6e656f2d6f6e65"}],"vin":[{"txid":"0x60d1404c2f4008d6cea42ff357c0ad1930c893ce26d1a898527efabafd067ba0","vout":1},{"txid":"0x976ec1556314ebab3c8ad99e133f9aeb56d730a47efabb2b224ddb1b036a474e","vout":0},{"txid":"0x848bb378f103b68d59b4c616f0be8e89b2fc931219a0de849363edccce9f8cd4","vout":0}],"vout":[{"n":0,"asset":"0xc56f33fc6ecfcd0c225c4ab356fee59390af8560be0e930faebe74a6daff7c9b","value":"33","address":"AHgozj1reiiBRh58nhSRUc2pmgLPNTSmcZ"}],"sys_fee":"0","net_fee":"0","scripts":[{"invocation":"40693f36f0f279b7aeff817a9440044599c43228ad692ef9de5d3760cf13e4ae55af49f8bb2c80446b81bb6a4c019a78cc195d06d7e81dd1f06edbd37801d4c664","verification":"2102fd7cbe089912347cfb699518452755db65f8c17d7232f8fd442700d2257986f4ac"}],"script":"51","gas":"0","blockindex":3639643}]}| |
