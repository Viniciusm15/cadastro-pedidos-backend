﻿using Domain.Models.Base;

namespace Domain.Models.ResponseModels
{
    public class ProductResponseModel : ProductBaseModel
    {
        public int ProductId { get; set; }
        public int ImageId { get; set; }
    }
}
