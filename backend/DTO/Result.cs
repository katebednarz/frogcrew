using System.ComponentModel.DataAnnotations;

namespace backend.DTO;

public class Result {
    

    public bool Flag { get; set; }

    public int Code { get; set; }

    public string Message { get; set; }
    public Object? Data { get; set; }

    public Result(bool flag, int code, string message) {
      Flag = flag;
      Code = code;
      Message = message;
    }

    public Result(bool flag, int code, string message, Object data) {
      Flag = flag;
      Code = code;
      Message = message;
      Data = data;
    }

    public override string ToString()
    {
        return $"Flag: {Flag}, Code: {Code}, Message: {Message}, Data: {Data}";
    }
}
