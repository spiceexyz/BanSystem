'use strict';

var config = require('./config');

var express = require('express')();
var bodyParser = require('body-parser');
var mongoose = require('mongoose');
var sanitise = require('express-mongo-sanitize');

app.use(bodyParser.json());
app.use(bodyParser.urlencoded({extended: true}));
app.use(sanitise());

express.listen(config.express.port);

express.post(config.express.directory, (request, response) => {

});