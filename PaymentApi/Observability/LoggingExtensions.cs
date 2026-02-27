using Amazon.Kinesis.Model;

Consumer.Received += async (model, ea) =>
{
    var json =
    Encoding.UTF8.GetString(ea.Body.ToArray());

    var client =
    httpClientFactory.CreateClient("gateway");

    var response =
    await client.PostAsync(
    "http://gateway/gateway", null);

};