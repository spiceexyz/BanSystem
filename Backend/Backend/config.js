module.exports = {
	mongoose: {
		host: 'localhost',
		port: 27017,
		auth: {
			db: 'admin',
			user: 'user',
			pass: 'pass'
		}
	},
	steam: {
		returnUrl: 'http://localhost:80/adminPanel',
		realm: 'http://localhost:80/',
		apiKey: 'XXXXXXXXXXXXXXXXXXXXXXXXXXX'
	}
}