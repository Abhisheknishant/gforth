/* vm interpreter wrapper

  Copyright (C) 2001 Free Software Foundation, Inc.

  This file is part of Gforth.

  Gforth is free software; you can redistribute it and/or
  modify it under the terms of the GNU General Public License
  as published by the Free Software Foundation; either version 2
  of the License, or (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with this program; if not, write to the Free Software
  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111, USA.
*/

#include "mini.h"

/* type change macros; these are specific to the types you use, so you
   have to change this part */
#define vm_Cell2i(x) ((Cell)(x))
#define vm_i2Cell(x) (x)
#define vm_Cell2target(x) ((Inst *)(x))
#define vm_target2Cell(x) ((Cell)(x))
#define vm_Cell2a(x) ((char *)(x))
#define vm_a2Cell(x) ((Cell)(x))

#ifdef USE_spTOS
#define IF_spTOS(x) x
#else
#define IF_spTOS(x)
#endif

#ifdef VM_DEBUG
#define NAME(_x) if (vm_debug) {fprintf(vm_out, "%lx: %-20s, ", (long)(ip-1), _x); fprintf(vm_out,"fp=%p, sp=%p", fp, sp);}
#else
#define NAME(_x)
#endif

/* different threading schemes for different architectures; for more,
   look in ../engine/threaded.h, and 
   look for THREADING_SCHEME in ../arch/.../machine.h */

#ifdef i386
#define THREADING_SCHEME 8
#else
#define THREADING_SCHEME 5
#endif

#if THREADING_SCHEME==5
/* direct threading scheme 5: early fetching */
#  define CFA_NEXT
#  define NEXT_P0	({cfa=*ip;})
#  define IP		((Cell *)ip)
#  define SET_IP(p)	({ip=(Inst *)(p); NEXT_P0;})
#  define NEXT_INST	((Cell)cfa)
#  define INC_IP(const_inc)	({cfa=ip[const_inc]; ip+=(const_inc);})
#  define DEF_CA
#  define NEXT_P1	(ip++)
#  define NEXT_P2	({goto *cfa;})
#endif

#if THREADING_SCHEME==8
/* direct threading scheme 8: i386 hack */
#  define NEXT_P0
#  define IP		(ip)
#  define SET_IP(p)	({ip=(p); NEXT_P0;})
#  define NEXT_INST	(*IP)
#  define INC_IP(const_inc)	({ ip+=(const_inc);})
#  define DEF_CA
#  define NEXT_P1	(ip++)
#  define NEXT_P2	({goto **(ip-1);})
#endif

#define NEXT ({DEF_CA NEXT_P1; NEXT_P2;})
#define IPTOS NEXT_INST

#ifdef VM_PROFILING
#define SUPER_END  vm_count_block(IP)
#else
#define SUPER_END
#endif

/* the return type can be anything you want it to */
Cell engine(Inst *ip0, Cell *sp, char *fp)
{
  /* VM registers */
  Inst * ip;
  Inst * cfa;
#ifdef USE_spTOS
  Cell   spTOS;
#else
#define spTOS (sp[0])
#endif
  static Inst   labels[] = {
#include "mini-labels.i"
  };

  if (vm_debug)
      fprintf(vm_out,"entering engine(%p,%p,%p)\n",ip0,sp,fp);
  if (ip0 == NULL) {
    vm_prim = labels;
    return 0;
  }

  /* I don't have a clue where these things come from,
     but I've put them in macros.h for the moment */
  IF_spTOS(spTOS = sp[0]);

  SET_IP(ip0);
  NEXT;

#include "mini-vm.i"
}
