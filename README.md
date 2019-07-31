# API文档说明
## 1.区块查询相关API
主链地址：https://api.alchemint.io/api/mainnet

测试链地址：https://api.alchemint.io/api/testnet

私链地址：https://api.alchemint.io/api/pri

以下所有接口所有环境都通用

###  getblock/获取区块高度
|                |                               |  remark                       |
|----------------|-------------------------------|-------------------------------|
|description     |Get block number               |                               |
|request         |{"jsonrpc": "2.0","method": "getblockcount","params": [],"id": 1}| params is null|
|response        |{"jsonrpc":"2.0","id":1,"result":[{"blockcount":"3661001"}]}     |               |

###  getnotify/获取日志记录
|                |                               |  remark                       |
|----------------|-------------------------------|-------------------------------|
|description     |Get application log by txid    |                               |
|request         |{"jsonrpc": "2.0","method": "getnotify","params":["f9f55031239475cff453ca3d8a2b4a61d6ad6cfbbbeae7770f10e3e357528d0c"],"id": 1}| params is txid|
|response        |{"jsonrpc":"2.0","id":1,"result":""}                          |                  |

###  getTxByTxid/获取交易详情
|                |                               |  remark                       |
|----------------|-------------------------------|-------------------------------|
|description     |Get transaction log by txid    |                               |
|request         |{"jsonrpc": "2.0","method": "getTxByTxid","params": ["f9f55031239475cff453ca3d8a2b4a61d6ad6cfbbbeae7770f10e3e357528d0c"],"id": 1}| params is txid|
|response        |{"jsonrpc":"2.0","id":1,"result":""}                           |                 

###  getsar4CDetailList/获取SAR4B列表详细信息
|                |                               |  remark                       |
|----------------|-------------------------------|-------------------------------|
|description     |Get sar list detail            |                               |
|request         |{"jsonrpc": "2.0","method": "getsar4CDetailList","params": [1,"0xb4df68fd65a3ce11c7d55325f95845be100b9710",10,1],"id": 1}||
|response        |{"jsonrpc":"2.0","id":1,"result":""}                           |     

###  getsar4BDetailList/获取SAR4B列表详细信息
|                |                               |  remark                       |
|----------------|-------------------------------|-------------------------------|
|description     |Get sar list detail            |                               |
|request         |{"jsonrpc": "2.0","method": "getsar4BDetailList","params": ["1","0x203205eef5a81c093d04b5c591d8f2308598caa2",10,1],"id": 1}||
|response        |{"jsonrpc":"2.0","id":1,"result":""}                           |  

###  getsar4CDetailByAdd/获取SAR4C详细信息
|                |                               |  remark                       |
|----------------|-------------------------------|-------------------------------|
|description     |Get sar detail                 |                               |
|request         |{"jsonrpc": "2.0","method": "getsar4CDetailByAdd","params": ["AaDEkefkq51LkgVuVd1Y5Gmd6rcsXaneJp","0xb4df68fd65a3ce11c7d55325f95845be100b9710"],"id": 1}||
|response        |{"jsonrpc":"2.0","id":1,"result":""}                           | 

###  getsar4BDetailByAdd/获取SAR4B详细信息
|                |                               |  remark                       |
|----------------|-------------------------------|-------------------------------|
|description     |Get sar detail                 |                               |
|request         |{"jsonrpc": "2.0","method": "getsar4BDetailByAdd","params": ["AaDEkefkq51LkgVuVd1Y5Gmd6rcsXaneJp","0xb4df68fd65a3ce11c7d55325f95845be100b9710"],"id": 1}||
|response        |{"jsonrpc":"2.0","id":1,"result":""}                           | 

###  getsar4COperatedBySAR/获取SAR4C操作记录
|                |                               |  remark                       |
|----------------|-------------------------------|-------------------------------|
|description     |Get sar list detail            |                               |
|request         |{"jsonrpc": "2.0","method": "getsar4CDetailByAdd","params": ["0x7b4bf9ceeb51142bf7e80c4c082e6479e87f66bfae8238bcbee1da9347892134",100,1],"id": 1}||
|response        |{"jsonrpc":"2.0","id":1,"result":""}                           | 

###  getStaticReport/获取SAR统计信息
|                |                               |  remark                       |
|----------------|-------------------------------|-------------------------------|
|description     |Get static data detail         |                               |
|request         |{"jsonrpc": "2.0","method": "getStaticReport","params": [],"id": 1}||
|response        |{"jsonrpc":"2.0","id":1,"result":""}                           | 

###  getUsableUtxoForNEO/获取未被使用过的UTXO
|                |                               |  remark                       |
|----------------|-------------------------------|-------------------------------|
|description     |Get utxo list                  |                               |
|request         |{"jsonrpc": "2.0","method": "getUsableUtxoForNEO","params": [],"id": 1}||
|response        |{"jsonrpc":"2.0","id":1,"result":""}                           | 

###  processSARFilterByRate/获取符合条件的SAR
|                |                               |  remark                       |
|----------------|-------------------------------|-------------------------------|
|description     |Get sar list                   |                               |
|request         |{"jsonrpc": "2.0","method": "processSARFilterByRate","params": [10,160],"id": 1}||
|response        |{"jsonrpc":"2.0","id":1,"result":""}                           | 

###  predictFeeTotal/获取预计手续总量
|                |                               |  remark                       |
|----------------|-------------------------------|-------------------------------|
|description     |Get sar list                   |                               |
|request         |{"jsonrpc": "2.0","method": "predictFeeTotal","params": [],"id": 1}||
|response        |{"jsonrpc":"2.0","id":1,"result":""}                           | 

###  addAuctionGoods/增加拍卖物品
|                |                               |  remark                       |
|----------------|-------------------------------|-------------------------------|
|description     |Add auction goods                   |                               |
|request         |{"jsonrpc": "2.0","method": "addAuctionGoods","params": ["0x1e3482d24740f2629fff407817cdc11bc2f1ae02","0xdbea4a60eab7ef53ca38f4943b8813650b655f5059b59a3836fed3660e3f0d99"],"id": 1}|goods合约/goods txid|
|response        |{"jsonrpc":"2.0","id":1,"result":""}                           | 

###  setAuctionResult/发起拍卖
|                |                               |  remark                       |
|----------------|-------------------------------|-------------------------------|
|description     |start auction goods                   |                               |
|request         |{"jsonrpc": "2.0","method": "setAuctionResult","params": ["0x5fa5aa68cf3330923463351f336f8caf95188ab5","AGa8mQumgxCfWUTWzpLVA77p1NMNw6qBwn","2019/7/30 17:06:43","1"],"id": 1}|account合约/address/auction key/auction mount|
|response        |{"jsonrpc":"2.0","id":1,"result":""}                           | 

###  getAuctionByStatus/查询拍卖物品
|                |                               |  remark                       |
|----------------|-------------------------------|-------------------------------|
|description     |query auction goods                   |                               |
|request         |{"jsonrpc": "2.0","method": "getAuctionByStatus","params": [10,1],"id": 1}|pages/index|
|response        |{"jsonrpc":"2.0","id":1,"result":""}                           | 

###  getAuctionByStatusCount/查询拍卖物品总数
|                |                               |  remark                       |
|----------------|-------------------------------|-------------------------------|
|description     |query auction goods                   |                               |
|request         |{"jsonrpc": "2.0","method": "getAuctionByStatusCount","params": [],"id": 1}|pages/index|
|response        |{"jsonrpc":"2.0","id":1,"result":""}                           | 

###  getAuctionRecords/根据地址查询拍卖记录
|                |                               |  remark                       |
|----------------|-------------------------------|-------------------------------|
|description     |query auction record                   |                               |
|request         |{"jsonrpc": "2.0","method": "getAuctionRecords","params": ["AGa8mQumgxCfWUTWzpLVA77p1NMNw6qBwn",10,1],"id": 1}|addr/pages/index|
|response        |{"jsonrpc":"2.0","id":1,"result":""}                           | 

###  getAuctionRecordsCount/根据地址查询拍卖记录
|                |                               |  remark                       |
|----------------|-------------------------------|-------------------------------|
|description     |query auction record                   |                               |
|request         |{"jsonrpc": "2.0","method": "getAuctionRecordsCount","params": ["AGa8mQumgxCfWUTWzpLVA77p1NMNw6qBwn"],"id": 1}|addr|
|response        |{"jsonrpc":"2.0","id":1,"result":""}                           | 

###  getAuctionRecordsByName/根据拍卖物查询拍卖记录
|                |                               |  remark                       |
|----------------|-------------------------------|-------------------------------|
|description     |query auction record                   |                               |
|request         |{"jsonrpc": "2.0","method": "getAuctionRecordsByName","params": ["2019/7/30 17:06:43",10,1],"id": 1}|key/pages/index|
|response        |{"jsonrpc":"2.0","id":1,"result":""}                           | 

###  getAuctionRecordsByNameCount/根据拍卖物查询拍卖记录
|                |                               |  remark                       |
|----------------|-------------------------------|-------------------------------|
|description     |query auction record                   |                               |
|request         |{"jsonrpc": "2.0","method": "getAuctionRecordsByNameCount","params": ["2019/7/30 17:06:43"],"id": 1}|key|
|response        |{"jsonrpc":"2.0","id":1,"result":""}                           | 

###  getBidPriceByAddr/查询当前拍卖最高价
|                |                               |  remark                       |
|----------------|-------------------------------|-------------------------------|
|description     |query auction record                   |                               |
|request         |{"jsonrpc": "2.0","method": "getBidPriceByAddr","params": ["2019/7/30 17:06:43","AGa8mQumgxCfWUTWzpLVA77p1NMNw6qBwn"],"id": 1}|key|
|response        |{"jsonrpc":"2.0","id":1,"result":""}                           | 




