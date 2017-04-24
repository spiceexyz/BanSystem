var mongoose = require('mongoose');

module.exports = mongoose.model('ServerOwner', new mongoose.Schema({
	SteamId: String,
	ApiKey: String
}));