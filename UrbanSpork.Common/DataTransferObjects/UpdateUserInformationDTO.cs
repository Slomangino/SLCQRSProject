﻿using System;
using System.Collections.Generic;
using System.Text;

namespace UrbanSpork.Common.DataTransferObjects
{
    public class UpdateUserInformationDTO
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Position { get; set; }
        public string Department { get; set; }
        public bool IsAdmin { get; set; }
    }
}