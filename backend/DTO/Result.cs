namespace backend.DTO;

public class Result {
    public required bool Flag { get; set; }
    public required int Code { get; set; }
    public required string Message { get; set; }
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
}
