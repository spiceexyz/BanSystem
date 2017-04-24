var express = require('express');
var router = express.Router();

router.get('/', (request, response, next) => {
	response.render('index');
});

module.exports = router;