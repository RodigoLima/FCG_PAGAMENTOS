﻿
namespace FCGPagamentos.Domain.Models;
public class BaseModel
{
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public DateTime? UpdatedAt { get; set; }
}