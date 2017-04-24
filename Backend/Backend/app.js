'use strict';

var config = require('./config');
var express = require('express');
var path = require('path');
var bodyParser = require('body-parser');
var sanitise = require('express-mongo-sanitize');

var app = express();

app.set('view engine', 'pug');
app.use(bodyParser.json());
app.use(bodyParser.urlencoded({extended: true}));
app.use(sanitise());

app.use(express.static(path.join(__dirname, 'public')));

app.use('/', require('./routes/index'));
app.use('/addBan', require('./routes/addBan'));

app.use((request, response, next) => {
	var err = new Error('Not Found');
	err.status = 404;
	next(err);
});

app.use((error, request, response, next) => {
	response.locals.message = error.message;
	response.locals.error = request.app.get('env') === 'development' ? error : {};
	response.status(error.status || 500);
	response.render('error');
});

app.listen(80);

module.exports = app;