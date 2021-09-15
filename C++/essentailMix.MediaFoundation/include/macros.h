#pragma once

#define NAME_OF(val) return #val
#define NAME_ASSIGN(param, val, str) if (param == val) str.assign(#val)
