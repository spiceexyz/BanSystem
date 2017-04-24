module.exports = {
	express: {
		port: 1024,
		directory: '/'
	},
	mongoose: {
		host: 'localhost',
		port: 27017,
		auth: {
			db: 'admin',
			user: 'user',
			pass: 'pass'
		}
	}
}