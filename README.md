# HMAC Authentication

For .net core projects.

HMAC authentication handler to eg. secure your Github webhooks.
Verifies both the data integrity and the authenticity of a message.

### Add to your Startup.cs
```c#
	public void ConfigureServices(IServiceCollection services)
	{
		services.AddHttpContextAccessor();

		services.Configure<HMACSignatureOptions>(Configuration.GetSection("Webhook"));
		services.AddAuthentication(o => { o.DefaultScheme = "Webhook"; }).AddScheme<HMACSignatureHandler>("Webhook");
	}
```

### To your configuration eg. appsettings.json
```json
"Webhook": {
  "Secret": "secret",
  "Header": "X-Hub-Signature", 
  "HashFunction":  1
},
```
- _**Header** defaults to X-Hub-Signature if left empty._
- _**HashFunction** is (SHA) hash function - 1, 256 or 512, Defaults to 1 if left empty._


### Secure your API :rocket:
```c#
	[Authorize(AuthenticationSchemes = "Webhook")]
	[HttpPost("api/release")]
	public async Task<IActionResult> Webhook()
	{
		return Ok();
	}
```
