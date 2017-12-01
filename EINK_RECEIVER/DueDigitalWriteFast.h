#pragma once

#ifndef BIT_READ
# define BIT_READ(value, bit)            ((value) &   (1UL << (bit)))
#endif
#ifndef BIT_SET
# define BIT_SET(value, bit)             ((value) |=  (1UL << (bit)))
#endif
#ifndef BITS_OFFSET
# define BITS_OFFSET(value, offset)         ((value) << (offset))
#endif
#ifndef BIT_CLEAR
# define BIT_CLEAR(value, bit)           ((value) &= ~(1UL << (bit)))
#endif
#ifndef BIT_WRITE
# define BIT_WRITE(value, bit, bitvalue) (bitvalue ? BIT_SET(value, bit) : BIT_CLEAR(value, bit))
#endif

#define __digitalPinToPortRegSODR(P) \
(\
((P) == 0 || (P) == 1 || \
((P) >= 16 && (P) <= 19) || \
(P) == 23 || (P) == 24 || \
(P) == 31 || \
(P) == 42 || (P) == 43 || \
((P) >= 54 && (P) <= 61) || \
((P) >= 68 && (P) <= 71) || \
((P) >= 73 && (P) <= 77) || \
(P) == 87 \
)? &REG_PIOA_SODR : \
(\
((P) == 2 || \
(P) == 13 || \
((P) >= 20 && (P) <= 22) || \
(P) == 52 || (P) == 53 || \
((P) >= 62 && (P) <= 67) \
)? &REG_PIOB_SODR : \
(\
(((P) >= 3 && (P) <= 10) || \
((P) >= 33 && (P) <= 41) || \
((P) >= 44 && (P) <= 51) || \
(P) == 72 \
)? &REG_PIOC_SODR : \
(\
((P) == 11 || (P) == 12 || \
(P) == 14 || (P) == 15 || \
((P) >= 25 && (P) <= 30) || \
(P) == 32 \
)? &REG_PIOD_SODR : &REG_PIOD_SODR \
))))

#define __digitalPinToPortRegCODR(P) \
(\
((P) == 0 || (P) == 1 || \
((P) >= 16 && (P) <= 19) || \
(P) == 23 || (P) == 24 || \
(P) == 31 || \
(P) == 42 || (P) == 43 || \
((P) >= 54 && (P) <= 61) || \
((P) >= 68 && (P) <= 71) || \
((P) >= 73 && (P) <= 77) || \
(P) == 87 \
)? &REG_PIOA_CODR : \
(\
((P) == 2 || \
(P) == 13 || \
((P) >= 20 && (P) <= 22) || \
(P) == 52 || (P) == 53 || \
((P) >= 62 && (P) <= 67) \
)? &REG_PIOB_CODR : \
(\
(((P) >= 3 && (P) <= 10) || \
((P) >= 33 && (P) <= 41) || \
((P) >= 44 && (P) <= 51) || \
(P) == 72 \
)? &REG_PIOC_CODR : \
(\
((P) == 11 || (P) == 12 || \
(P) == 14 || (P) == 15 || \
((P) >= 25 && (P) <= 30) || \
(P) == 32 \
)? &REG_PIOD_CODR : &REG_PIOD_CODR \
))))

#define __digitalPinToPortRegOWER(P) \
(\
((P) == 0 || (P) == 1 || \
((P) >= 16 && (P) <= 19) || \
(P) == 23 || (P) == 24 || \
(P) == 31 || \
(P) == 42 || (P) == 43 || \
((P) >= 54 && (P) <= 61) || \
((P) >= 68 && (P) <= 71) || \
((P) >= 73 && (P) <= 77) || \
(P) == 87 \
)? &REG_PIOA_OWER : \
(\
((P) == 2 || \
(P) == 13 || \
((P) >= 20 && (P) <= 22) || \
(P) == 52 || (P) == 53 || \
((P) >= 62 && (P) <= 67) \
)? &REG_PIOB_OWER : \
(\
(((P) >= 3 && (P) <= 10) || \
((P) >= 33 && (P) <= 41) || \
((P) >= 44 && (P) <= 51) || \
(P) == 72 \
)? &REG_PIOC_OWER : \
(\
((P) == 11 || (P) == 12 || \
(P) == 14 || (P) == 15 || \
((P) >= 25 && (P) <= 30) || \
(P) == 32 \
)? &REG_PIOD_OWER : &REG_PIOD_OWER \
))))

#define __digitalPinToPortRegOWDR(P) \
(\
((P) == 0 || (P) == 1 || \
((P) >= 16 && (P) <= 19) || \
(P) == 23 || (P) == 24 || \
(P) == 31 || \
(P) == 42 || (P) == 43 || \
((P) >= 54 && (P) <= 61) || \
((P) >= 68 && (P) <= 71) || \
((P) >= 73 && (P) <= 77) || \
(P) == 87 \
)? &REG_PIOA_OWDR : \
(\
((P) == 2 || \
(P) == 13 || \
((P) >= 20 && (P) <= 22) || \
(P) == 52 || (P) == 53 || \
((P) >= 62 && (P) <= 67) \
)? &REG_PIOB_OWDR : \
(\
(((P) >= 3 && (P) <= 10) || \
((P) >= 33 && (P) <= 41) || \
((P) >= 44 && (P) <= 51) || \
(P) == 72 \
)? &REG_PIOC_OWDR : \
(\
((P) == 11 || (P) == 12 || \
(P) == 14 || (P) == 15 || \
((P) >= 25 && (P) <= 30) || \
(P) == 32 \
)? &REG_PIOD_OWDR : &REG_PIOD_OWDR \
))))

#define __digitalPinToPortRegODSR(P) \
(\
((P) == 0 || (P) == 1 || \
((P) >= 16 && (P) <= 19) || \
(P) == 23 || (P) == 24 || \
(P) == 31 || \
(P) == 42 || (P) == 43 || \
((P) >= 54 && (P) <= 61) || \
((P) >= 68 && (P) <= 71) || \
((P) >= 73 && (P) <= 77) || \
(P) == 87 \
)? &REG_PIOA_ODSR : \
(\
((P) == 2 || \
(P) == 13 || \
((P) >= 20 && (P) <= 22) || \
(P) == 52 || (P) == 53 || \
((P) >= 62 && (P) <= 67) \
)? &REG_PIOB_ODSR : \
(\
(((P) >= 3 && (P) <= 10) || \
((P) >= 33 && (P) <= 41) || \
((P) >= 44 && (P) <= 51) || \
(P) == 72 \
)? &REG_PIOC_ODSR : \
(\
((P) == 11 || (P) == 12 || \
(P) == 14 || (P) == 15 || \
((P) >= 25 && (P) <= 30) || \
(P) == 32 \
)? &REG_PIOD_ODSR : &REG_PIOD_ODSR \
))))


#define __digitalPinToBit(P) \
((P) == 69 || (P) == 25) ? 0 : \
(((P) == 68 || (P) == 33 || (P) == 26) ? 1 : \
(((P) == 61 || (P) == 34 || (P) == 27) ? 2 : \
(((P) == 60 || (P) == 35 || (P) == 28) ? 3 : \
\
(((P) == 59 || (P) == 36 || (P) == 14) ? 4 : \
(((P) == 37 || (P) == 15) ? 5 : \
(((P) == 58 || (P) == 38 || (P) == 29) ? 6 : \
(((P) == 31 || (P) == 39 || (P) == 11) ? 7 : \
\
(((P) == 0 || (P) == 40 || (P) == 12) ? 8 : \
(((P) == 1 || (P) == 41 || (P) == 30) ? 9 : \
(((P) == 19 || (P) == 32) ? 10 : \
(((P) == 18) ? 11 : \
\
(((P) == 17 || (P) == 20 || (P) == 51) ? 12 : \
(((P) == 16 || (P) == 21 || (P) == 50) ? 13 : \
(((P) == 23 || (P) == 53 || (P) == 49) ? 14 : \
(((P) == 24 || (P) == 66 || (P) == 48) ? 15 : \
\
(((P) == 54 || (P) == 67 || (P) == 47) ? 16 : \
(((P) == 70 || (P) == 62 || (P) == 46) ? 17 : \
(((P) == 71 || (P) == 63 || (P) == 45) ? 18 : \
(((P) == 42 || (P) == 64 || (P) == 44) ? 19 : \
\
(((P) == 43 || (P) == 65) ? 20 : \
(((P) == 73 || (P) == 52 || (P) == 9) ? 21 : \
(((P) == 57 || (P) == 8) ? 22 : \
(((P) == 56 || (P) == 7) ? 23 : \
\
(((P) == 55 || (P) == 6) ? 24 : \
(((P) == 76 || (P) == 2 || (P) == 5) ? 25 : \
(((P) == 75 || (P) == 22 || (P) == 4) ? 26 : \
(((P) == 74 || (P) == 13) ? 27 : \
\
(((P) == 77 || (P) == 3) ? 28 : \
(((P) == 87 || (P) == 10) ? 29 : \
(((P) == 72) ? 30 : -1\
\
))))))))))))))))))))))))))))))

void NonConstantUsed(void)  __attribute__((error("")));

#define digitalWriteFast(P, V) \
digitalWrite(P,V);

//if (__builtin_constant_p(P)) \
 { \
	(V == 1) ? \
		BIT_SET(*__digitalPinToPortRegSODR(P), __digitalPinToBit(P)) \
		: BIT_SET(*__digitalPinToPortRegCODR(P), __digitalPinToBit(P)); \
} \
else { \
  NonConstantUsed(); \
}


#define DirectRegisterEnable(P) \
if (__builtin_constant_p(P)) { \
  BIT_SET(*__digitalPinToPortRegOWER(P), __digitalPinToBit(P)); \
} else { \
  NonConstantUsed(); \
}

#define DirectRegisterDisable(P) \
if (__builtin_constant_p(P)) { \
  BIT_SET(*__digitalPinToPortRegOWDR(P), __digitalPinToBit(P)); \
} else { \
  NonConstantUsed(); \
}

