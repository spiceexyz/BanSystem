var express = require('express');
var router = express.Router();

router.get('/', (request, response, next) => {
	var apiKey = request.query.apiKey;
	response.send(apiKey);
});

module.exports = router;