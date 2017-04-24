var config = require('../config');
var express = require('express');
var passport = require('passport');
var SteamStrategy = require('passport-steam').Strategy;
var router = express.Router();

passport.use(new SteamStrategy({
	returnURL: config.steam.returnUrl,
	realm: config.steam.realm,
	apiKey: config.steam.apiKey
}, (identifier, profile, done) => {
		User.findByOpenID({ openId: identifier }, (err, user) => {
			return done(err, user);
		});
	}
));

router.get('/', passport.authenticate('steam'), (request, response) => {

});

module.exports = router;