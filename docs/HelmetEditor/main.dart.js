(function dartProgram(){function copyProperties(a,b){var s=Object.keys(a)
for(var r=0;r<s.length;r++){var q=s[r]
b[q]=a[q]}}function mixinPropertiesHard(a,b){var s=Object.keys(a)
for(var r=0;r<s.length;r++){var q=s[r]
if(!b.hasOwnProperty(q)){b[q]=a[q]}}}function mixinPropertiesEasy(a,b){Object.assign(b,a)}var z=function(){var s=function(){}
s.prototype={p:{}}
var r=new s()
if(!(Object.getPrototypeOf(r)&&Object.getPrototypeOf(r).p===s.prototype.p))return false
try{if(typeof navigator!="undefined"&&typeof navigator.userAgent=="string"&&navigator.userAgent.indexOf("Chrome/")>=0)return true
if(typeof version=="function"&&version.length==0){var q=version()
if(/^\d+\.\d+\.\d+\.\d+$/.test(q))return true}}catch(p){}return false}()
function inherit(a,b){a.prototype.constructor=a
a.prototype["$i"+a.name]=a
if(b!=null){if(z){Object.setPrototypeOf(a.prototype,b.prototype)
return}var s=Object.create(b.prototype)
copyProperties(a.prototype,s)
a.prototype=s}}function inheritMany(a,b){for(var s=0;s<b.length;s++){inherit(b[s],a)}}function mixinEasy(a,b){mixinPropertiesEasy(b.prototype,a.prototype)
a.prototype.constructor=a}function mixinHard(a,b){mixinPropertiesHard(b.prototype,a.prototype)
a.prototype.constructor=a}function lazy(a,b,c,d){var s=a
a[b]=s
a[c]=function(){if(a[b]===s){a[b]=d()}a[c]=function(){return this[b]}
return a[b]}}function lazyFinal(a,b,c,d){var s=a
a[b]=s
a[c]=function(){if(a[b]===s){var r=d()
if(a[b]!==s){A.oy(b)}a[b]=r}var q=a[b]
a[c]=function(){return q}
return q}}function makeConstList(a,b){if(b!=null)A.d(a,b)
a.$flags=7
return a}function convertToFastObject(a){function t(){}t.prototype=a
new t()
return a}function convertAllToFastObject(a){for(var s=0;s<a.length;++s){convertToFastObject(a[s])}}var y=0
function instanceTearOffGetter(a,b){var s=null
return a?function(c){if(s===null)s=A.jg(b)
return new s(c,this)}:function(){if(s===null)s=A.jg(b)
return new s(this,null)}}function staticTearOffGetter(a){var s=null
return function(){if(s===null)s=A.jg(a).prototype
return s}}var x=0
function tearOffParameters(a,b,c,d,e,f,g,h,i,j){if(typeof h=="number"){h+=x}return{co:a,iS:b,iI:c,rC:d,dV:e,cs:f,fs:g,fT:h,aI:i||0,nDA:j}}function installStaticTearOff(a,b,c,d,e,f,g,h){var s=tearOffParameters(a,true,false,c,d,e,f,g,h,false)
var r=staticTearOffGetter(s)
a[b]=r}function installInstanceTearOff(a,b,c,d,e,f,g,h,i,j){c=!!c
var s=tearOffParameters(a,false,c,d,e,f,g,h,i,!!j)
var r=instanceTearOffGetter(c,s)
a[b]=r}function setOrUpdateInterceptorsByTag(a){var s=v.interceptorsByTag
if(!s){v.interceptorsByTag=a
return}copyProperties(a,s)}function setOrUpdateLeafTags(a){var s=v.leafTags
if(!s){v.leafTags=a
return}copyProperties(a,s)}function updateTypes(a){var s=v.types
var r=s.length
s.push.apply(s,a)
return r}function updateHolder(a,b){copyProperties(b,a)
return a}var hunkHelpers=function(){var s=function(a,b,c,d,e){return function(f,g,h,i){return installInstanceTearOff(f,g,a,b,c,d,[h],i,e,false)}},r=function(a,b,c,d){return function(e,f,g,h){return installStaticTearOff(e,f,a,b,c,[g],h,d)}}
return{inherit:inherit,inheritMany:inheritMany,mixin:mixinEasy,mixinHard:mixinHard,installStaticTearOff:installStaticTearOff,installInstanceTearOff:installInstanceTearOff,_instance_0u:s(0,0,null,["$0"],0),_instance_1u:s(0,1,null,["$1"],0),_instance_2u:s(0,2,null,["$2"],0),_instance_0i:s(1,0,null,["$0"],0),_instance_1i:s(1,1,null,["$1"],0),_instance_2i:s(1,2,null,["$2"],0),_static_0:r(0,null,["$0"],0),_static_1:r(1,null,["$1"],0),_static_2:r(2,null,["$2"],0),makeConstList:makeConstList,lazy:lazy,lazyFinal:lazyFinal,updateHolder:updateHolder,convertToFastObject:convertToFastObject,updateTypes:updateTypes,setOrUpdateInterceptorsByTag:setOrUpdateInterceptorsByTag,setOrUpdateLeafTags:setOrUpdateLeafTags}}()
function initializeDeferredHunk(a){x=v.types.length
a(hunkHelpers,v,w,$)}var J={
jq(a,b,c,d){return{i:a,p:b,e:c,x:d}},
fV(a){var s,r,q,p,o,n=a[v.dispatchPropertyName]
if(n==null)if($.jj==null){A.nN()
n=a[v.dispatchPropertyName]}if(n!=null){s=n.p
if(!1===s)return n.i
if(!0===s)return a
r=Object.getPrototypeOf(a)
if(s===r)return n.i
if(n.e===r)throw A.f(A.k2("Return interceptor for "+A.k(s(a,n))))}q=a.constructor
if(q==null)p=null
else{o=$.f1
if(o==null)o=$.f1=v.getIsolateTag("_$dart_js")
p=q[o]}if(p!=null)return p
p=A.nV(a)
if(p!=null)return p
if(typeof a=="function")return B.L
s=Object.getPrototypeOf(a)
if(s==null)return B.x
if(s===Object.prototype)return B.x
if(typeof q=="function"){o=$.f1
if(o==null)o=$.f1=v.getIsolateTag("_$dart_js")
Object.defineProperty(q,o,{value:B.u,enumerable:false,writable:true,configurable:true})
return B.u}return B.u},
lK(a,b){if(a<0||a>4294967295)throw A.f(A.ar(a,0,4294967295,"length",null))
return J.lM(new Array(a),b)},
lL(a,b){if(a<0)throw A.f(A.C("Length must be a non-negative integer: "+a,null))
return A.d(new Array(a),b.h("u<0>"))},
iM(a,b){if(a<0)throw A.f(A.C("Length must be a non-negative integer: "+a,null))
return A.d(new Array(a),b.h("u<0>"))},
lM(a,b){var s=A.d(a,b.h("u<0>"))
s.$flags=1
return s},
jM(a,b){var s=t.h
return J.lp(s.a(a),s.a(b))},
bn(a){if(typeof a=="number"){if(Math.floor(a)==a)return J.c3.prototype
return J.dc.prototype}if(typeof a=="string")return J.aS.prototype
if(a==null)return J.c4.prototype
if(typeof a=="boolean")return J.db.prototype
if(Array.isArray(a))return J.u.prototype
if(typeof a!="object"){if(typeof a=="function")return J.aF.prototype
if(typeof a=="symbol")return J.bx.prototype
if(typeof a=="bigint")return J.bw.prototype
return a}if(a instanceof A.x)return a
return J.fV(a)},
kO(a){if(typeof a=="string")return J.aS.prototype
if(a==null)return a
if(Array.isArray(a))return J.u.prototype
if(typeof a!="object"){if(typeof a=="function")return J.aF.prototype
if(typeof a=="symbol")return J.bx.prototype
if(typeof a=="bigint")return J.bw.prototype
return a}if(a instanceof A.x)return a
return J.fV(a)},
cN(a){if(a==null)return a
if(Array.isArray(a))return J.u.prototype
if(typeof a!="object"){if(typeof a=="function")return J.aF.prototype
if(typeof a=="symbol")return J.bx.prototype
if(typeof a=="bigint")return J.bw.prototype
return a}if(a instanceof A.x)return a
return J.fV(a)},
nB(a){if(typeof a=="number")return J.bv.prototype
if(typeof a=="string")return J.aS.prototype
if(a==null)return a
if(!(a instanceof A.x))return J.bd.prototype
return a},
nC(a){if(typeof a=="string")return J.aS.prototype
if(a==null)return a
if(!(a instanceof A.x))return J.bd.prototype
return a},
nD(a){if(a==null)return a
if(typeof a!="object"){if(typeof a=="function")return J.aF.prototype
if(typeof a=="symbol")return J.bx.prototype
if(typeof a=="bigint")return J.bw.prototype
return a}if(a instanceof A.x)return a
return J.fV(a)},
S(a,b){if(a==null)return b==null
if(typeof a!="object")return b!=null&&a===b
return J.bn(a).N(a,b)},
cZ(a,b){return J.cN(a).i(a,b)},
jE(a,b){return J.cN(a).C(a,b)},
lo(a,b){return J.nC(a).bk(a,b)},
iG(a,b,c){return J.nD(a).bo(a,b,c)},
lp(a,b){return J.nB(a).a6(a,b)},
iH(a,b){return J.cN(a).I(a,b)},
aw(a){return J.bn(a).gD(a)},
bU(a){return J.cN(a).gt(a)},
bt(a){return J.kO(a).gm(a)},
lq(a){return J.bn(a).gE(a)},
lr(a,b){return J.cN(a).H(a,b)},
jF(a,b,c){return J.cN(a).bt(a,b,c)},
d_(a){return J.bn(a).j(a)},
d9:function d9(){},
db:function db(){},
c4:function c4(){},
c6:function c6(){},
aT:function aT(){},
dr:function dr(){},
bd:function bd(){},
aF:function aF(){},
bw:function bw(){},
bx:function bx(){},
u:function u(a){this.$ti=a},
da:function da(){},
ed:function ed(a){this.$ti=a},
bV:function bV(a,b,c){var _=this
_.a=a
_.b=b
_.c=0
_.d=null
_.$ti=c},
bv:function bv(){},
c3:function c3(){},
dc:function dc(){},
aS:function aS(){}},A={iN:function iN(){},
aI(a,b){a=a+b&536870911
a=a+((a&524287)<<10)&536870911
return a^a>>>6},
eH(a){a=a+((a&67108863)<<3)&536870911
a^=a>>>11
return a+((a&16383)<<15)&536870911},
fP(a,b,c){return a},
jl(a){var s,r
for(s=$.ai.length,r=0;r<s;++r)if(a===$.ai[r])return!0
return!1},
lX(a,b,c,d){A.es(b,"start")
if(c!=null){A.es(c,"end")
if(b>c)A.M(A.ar(b,0,c,"start",null))}return new A.cm(a,b,c,d.h("cm<0>"))},
iJ(){return new A.cl("No element")},
bB:function bB(a){this.a=0
this.b=a},
de:function de(a){this.a=a},
et:function et(){},
bY:function bY(){},
O:function O(){},
cm:function cm(a,b,c,d){var _=this
_.a=a
_.b=b
_.c=c
_.$ti=d},
ad:function ad(a,b,c){var _=this
_.a=a
_.b=b
_.c=0
_.d=null
_.$ti=c},
cb:function cb(){},
bZ:function bZ(a,b,c){this.a=a
this.b=b
this.$ti=c},
cc:function cc(a,b,c){var _=this
_.a=null
_.b=a
_.c=b
_.$ti=c},
ae:function ae(a,b,c){this.a=a
this.b=b
this.$ti=c},
aL:function aL(a,b,c){this.a=a
this.b=b
this.$ti=c},
bf:function bf(a,b,c){this.a=a
this.b=b
this.$ti=c},
c_:function c_(a){this.$ti=a},
c0:function c0(a){this.$ti=a},
bA:function bA(a,b){this.a=a
this.$ti=b},
co:function co(a,b){this.a=a
this.$ti=b},
G:function G(){},
aH:function aH(a,b){this.a=a
this.$ti=b},
lz(){throw A.f(A.be("Cannot modify constant Set"))},
l8(a){var s=v.mangledGlobalNames[a]
if(s!=null)return s
return"minified:"+a},
p_(a,b){var s
if(b!=null){s=b.x
if(s!=null)return s}return t.da.b(a)},
k(a){var s
if(typeof a=="string")return a
if(typeof a=="number"){if(a!==0)return""+a}else if(!0===a)return"true"
else if(!1===a)return"false"
else if(a==null)return"null"
s=J.d_(a)
return s},
ds(a){var s,r=$.jW
if(r==null)r=$.jW=Symbol("identityHashCode")
s=a[r]
if(s==null){s=Math.random()*0x3fffffff|0
a[r]=s}return s},
dt(a){var s,r,q,p
if(a instanceof A.x)return A.ah(A.a6(a),null)
s=J.bn(a)
if(s===B.K||s===B.M||t.cr.b(a)){r=B.v(a)
if(r!=="Object"&&r!=="")return r
q=a.constructor
if(typeof q=="function"){p=q.name
if(typeof p=="string"&&p!=="Object"&&p!=="")return p}}return A.ah(A.a6(a),null)},
jX(a){var s,r,q
if(a==null||typeof a=="number"||A.j4(a))return J.d_(a)
if(typeof a=="string")return JSON.stringify(a)
if(a instanceof A.aR)return a.j(0)
if(a instanceof A.ag)return a.bi(!0)
s=$.lm()
for(r=0;r<1;++r){q=s[r].cm(a)
if(q!=null)return q}return"Instance of '"+A.dt(a)+"'"},
lR(a){var s=a.$thrownJsError
if(s==null)return null
return A.bR(s)},
jY(a,b){var s
if(a.$thrownJsError==null){s=new Error()
A.U(a,s)
a.$thrownJsError=s
s.stack=b.j(0)}},
nI(a){throw A.f(A.fI(a))},
a(a,b){if(a==null)J.bt(a)
throw A.f(A.fS(a,b))},
fS(a,b){var s,r="index"
if(!A.ks(b))return new A.ap(!0,b,r,null)
s=J.bt(a)
if(b<0||b>=s)return A.ec(b,s,a,null,r)
return A.jZ(b,r)},
nt(a,b,c){if(a<0||a>c)return A.ar(a,0,c,"start",null)
if(b!=null)if(b<a||b>c)return A.ar(b,a,c,"end",null)
return new A.ap(!0,b,"end",null)},
fI(a){return new A.ap(!0,a,null,null)},
f(a){return A.U(a,new Error())},
U(a,b){var s
if(a==null)a=new A.aJ()
b.dartException=a
s=A.oz
if("defineProperty" in Object){Object.defineProperty(b,"message",{get:s})
b.name=""}else b.toString=s
return b},
oz(){return J.d_(this.dartException)},
M(a,b){throw A.U(a,b==null?new Error():b)},
r(a,b,c){var s
if(b==null)b=0
if(c==null)c=0
s=Error()
A.M(A.mx(a,b,c),s)},
mx(a,b,c){var s,r,q,p,o,n,m,l,k
if(typeof b=="string")s=b
else{r="[]=;add;removeWhere;retainWhere;removeRange;setRange;setInt8;setInt16;setInt32;setUint8;setUint16;setUint32;setFloat32;setFloat64".split(";")
q=r.length
p=b
if(p>q){c=p/q|0
p%=q}s=r[p]}o=typeof c=="string"?c:"modify;remove from;add to".split(";")[c]
n=t.j.b(a)?"list":"ByteData"
m=a.$flags|0
l="a "
if((m&4)!==0)k="constant "
else if((m&2)!==0){k="unmodifiable "
l="an "}else k=(m&1)!==0?"fixed-length ":""
return new A.cn("'"+s+"': Cannot "+o+" "+l+k+n)},
o(a){throw A.f(A.Z(a))},
aK(a){var s,r,q,p,o,n
a=A.l1(a.replace(String({}),"$receiver$"))
s=a.match(/\\\$[a-zA-Z]+\\\$/g)
if(s==null)s=A.d([],t.s)
r=s.indexOf("\\$arguments\\$")
q=s.indexOf("\\$argumentsExpr\\$")
p=s.indexOf("\\$expr\\$")
o=s.indexOf("\\$method\\$")
n=s.indexOf("\\$receiver\\$")
return new A.eI(a.replace(new RegExp("\\\\\\$arguments\\\\\\$","g"),"((?:x|[^x])*)").replace(new RegExp("\\\\\\$argumentsExpr\\\\\\$","g"),"((?:x|[^x])*)").replace(new RegExp("\\\\\\$expr\\\\\\$","g"),"((?:x|[^x])*)").replace(new RegExp("\\\\\\$method\\\\\\$","g"),"((?:x|[^x])*)").replace(new RegExp("\\\\\\$receiver\\\\\\$","g"),"((?:x|[^x])*)"),r,q,p,o,n)},
eJ(a){return function($expr$){var $argumentsExpr$="$arguments$"
try{$expr$.$method$($argumentsExpr$)}catch(s){return s.message}}(a)},
k1(a){return function($expr$){try{$expr$.$method$}catch(s){return s.message}}(a)},
iO(a,b){var s=b==null,r=s?null:b.method
return new A.dd(a,r,s?null:b.receiver)},
aQ(a){var s
if(a==null)return new A.eo(a)
if(a instanceof A.c1){s=a.a
return A.b2(a,s==null?A.bF(s):s)}if(typeof a!=="object")return a
if("dartException" in a)return A.b2(a,a.dartException)
return A.ng(a)},
b2(a,b){if(t.C.b(b))if(b.$thrownJsError==null)b.$thrownJsError=a
return b},
ng(a){var s,r,q,p,o,n,m,l,k,j,i,h,g
if(!("message" in a))return a
s=a.message
if("number" in a&&typeof a.number=="number"){r=a.number
q=r&65535
if((B.a.B(r,16)&8191)===10)switch(q){case 438:return A.b2(a,A.iO(A.k(s)+" (Error "+q+")",null))
case 445:case 5007:A.k(s)
return A.b2(a,new A.cf())}}if(a instanceof TypeError){p=$.l9()
o=$.la()
n=$.lb()
m=$.lc()
l=$.lf()
k=$.lg()
j=$.le()
$.ld()
i=$.li()
h=$.lh()
g=p.L(s)
if(g!=null)return A.b2(a,A.iO(A.B(s),g))
else{g=o.L(s)
if(g!=null){g.method="call"
return A.b2(a,A.iO(A.B(s),g))}else if(n.L(s)!=null||m.L(s)!=null||l.L(s)!=null||k.L(s)!=null||j.L(s)!=null||m.L(s)!=null||i.L(s)!=null||h.L(s)!=null){A.B(s)
return A.b2(a,new A.cf())}}return A.b2(a,new A.dC(typeof s=="string"?s:""))}if(a instanceof RangeError){if(typeof s=="string"&&s.indexOf("call stack")!==-1)return new A.ck()
s=function(b){try{return String(b)}catch(f){}return null}(a)
return A.b2(a,new A.ap(!1,null,null,typeof s=="string"?s.replace(/^RangeError:\s*/,""):s))}if(typeof InternalError=="function"&&a instanceof InternalError)if(typeof s=="string"&&s==="too much recursion")return new A.ck()
return a},
bR(a){var s
if(a instanceof A.c1)return a.b
if(a==null)return new A.cA(a)
s=a.$cachedTrace
if(s!=null)return s
s=new A.cA(a)
if(typeof a==="object")a.$cachedTrace=s
return s},
kU(a){if(a==null)return J.aw(a)
if(typeof a=="object")return A.ds(a)
return J.aw(a)},
ny(a,b){var s,r,q,p=a.length
for(s=0;s<p;s=q){r=s+1
q=r+1
b.q(0,a[s],a[r])}return b},
nz(a,b){var s,r=a.length
for(s=0;s<r;++s)b.i(0,a[s])
return b},
mK(a,b,c,d,e,f){t.Z.a(a)
switch(A.j(b)){case 0:return a.$0()
case 1:return a.$1(c)
case 2:return a.$2(c,d)
case 3:return a.$3(c,d,e)
case 4:return a.$4(c,d,e,f)}throw A.f(new A.eR("Unsupported number of arguments for wrapped closure"))},
bO(a,b){var s=a.$identity
if(!!s)return s
s=A.no(a,b)
a.$identity=s
return s},
no(a,b){var s
switch(b){case 0:s=a.$0
break
case 1:s=a.$1
break
case 2:s=a.$2
break
case 3:s=a.$3
break
case 4:s=a.$4
break
default:s=null}if(s!=null)return s.bind(a)
return function(c,d,e){return function(f,g,h,i){return e(c,d,f,g,h,i)}}(a,b,A.mK)},
ly(a2){var s,r,q,p,o,n,m,l,k,j,i=a2.co,h=a2.iS,g=a2.iI,f=a2.nDA,e=a2.aI,d=a2.fs,c=a2.cs,b=d[0],a=c[0],a0=i[b],a1=a2.fT
a1.toString
s=h?Object.create(new A.dw().constructor.prototype):Object.create(new A.bu(null,null).constructor.prototype)
s.$initialize=s.constructor
r=h?function static_tear_off(){this.$initialize()}:function tear_off(a3,a4){this.$initialize(a3,a4)}
s.constructor=r
r.prototype=s
s.$_name=b
s.$_target=a0
q=!h
if(q)p=A.jL(b,a0,g,f)
else{s.$static_name=b
p=a0}s.$S=A.lu(a1,h,g)
s[a]=p
for(o=p,n=1;n<d.length;++n){m=d[n]
if(typeof m=="string"){l=i[m]
k=m
m=l}else k=""
j=c[n]
if(j!=null){if(q)m=A.jL(k,m,g,f)
s[j]=m}if(n===e)o=m}s.$C=o
s.$R=a2.rC
s.$D=a2.dV
return r},
lu(a,b,c){if(typeof a=="number")return a
if(typeof a=="string"){if(b)throw A.f("Cannot compute signature for static tearoff.")
return function(d,e){return function(){return e(this,d)}}(a,A.ls)}throw A.f("Error in functionType of tearoff")},
lv(a,b,c,d){var s=A.jK
switch(b?-1:a){case 0:return function(e,f){return function(){return f(this)[e]()}}(c,s)
case 1:return function(e,f){return function(g){return f(this)[e](g)}}(c,s)
case 2:return function(e,f){return function(g,h){return f(this)[e](g,h)}}(c,s)
case 3:return function(e,f){return function(g,h,i){return f(this)[e](g,h,i)}}(c,s)
case 4:return function(e,f){return function(g,h,i,j){return f(this)[e](g,h,i,j)}}(c,s)
case 5:return function(e,f){return function(g,h,i,j,k){return f(this)[e](g,h,i,j,k)}}(c,s)
default:return function(e,f){return function(){return e.apply(f(this),arguments)}}(d,s)}},
jL(a,b,c,d){if(c)return A.lx(a,b,d)
return A.lv(b.length,d,a,b)},
lw(a,b,c,d){var s=A.jK,r=A.lt
switch(b?-1:a){case 0:throw A.f(new A.du("Intercepted function with no arguments."))
case 1:return function(e,f,g){return function(){return f(this)[e](g(this))}}(c,r,s)
case 2:return function(e,f,g){return function(h){return f(this)[e](g(this),h)}}(c,r,s)
case 3:return function(e,f,g){return function(h,i){return f(this)[e](g(this),h,i)}}(c,r,s)
case 4:return function(e,f,g){return function(h,i,j){return f(this)[e](g(this),h,i,j)}}(c,r,s)
case 5:return function(e,f,g){return function(h,i,j,k){return f(this)[e](g(this),h,i,j,k)}}(c,r,s)
case 6:return function(e,f,g){return function(h,i,j,k,l){return f(this)[e](g(this),h,i,j,k,l)}}(c,r,s)
default:return function(e,f,g){return function(){var q=[g(this)]
Array.prototype.push.apply(q,arguments)
return e.apply(f(this),q)}}(d,r,s)}},
lx(a,b,c){var s,r
if($.jI==null)$.jI=A.jH("interceptor")
if($.jJ==null)$.jJ=A.jH("receiver")
s=b.length
r=A.lw(s,c,a,b)
return r},
jg(a){return A.ly(a)},
ls(a,b){return A.cF(v.typeUniverse,A.a6(a.a),b)},
jK(a){return a.a},
lt(a){return a.b},
jH(a){var s,r,q,p=new A.bu("receiver","interceptor"),o=Object.getOwnPropertyNames(p)
o.$flags=1
s=o
for(o=s.length,r=0;r<o;++r){q=s[r]
if(p[q]===a)return q}throw A.f(A.C("Field name "+a+" not found.",null))},
nE(a){return v.getIsolateTag(a)},
os(){return v.G},
oY(a,b,c){Object.defineProperty(a,b,{value:c,enumerable:false,writable:true,configurable:true})},
nV(a){var s,r,q,p,o,n=A.B($.kP.$1(a)),m=$.fT[n]
if(m!=null){Object.defineProperty(a,v.dispatchPropertyName,{value:m,enumerable:false,writable:true,configurable:true})
return m.i}s=$.h0[n]
if(s!=null)return s
r=v.interceptorsByTag[n]
if(r==null){q=A.iX($.kE.$2(a,n))
if(q!=null){m=$.fT[q]
if(m!=null){Object.defineProperty(a,v.dispatchPropertyName,{value:m,enumerable:false,writable:true,configurable:true})
return m.i}s=$.h0[q]
if(s!=null)return s
r=v.interceptorsByTag[q]
n=q}}if(r==null)return null
s=r.prototype
p=n[0]
if(p==="!"){m=A.hn(s)
$.fT[n]=m
Object.defineProperty(a,v.dispatchPropertyName,{value:m,enumerable:false,writable:true,configurable:true})
return m.i}if(p==="~"){$.h0[n]=s
return s}if(p==="-"){o=A.hn(s)
Object.defineProperty(Object.getPrototypeOf(a),v.dispatchPropertyName,{value:o,enumerable:false,writable:true,configurable:true})
return o.i}if(p==="+")return A.kZ(a,s)
if(p==="*")throw A.f(A.k2(n))
if(v.leafTags[n]===true){o=A.hn(s)
Object.defineProperty(Object.getPrototypeOf(a),v.dispatchPropertyName,{value:o,enumerable:false,writable:true,configurable:true})
return o.i}else return A.kZ(a,s)},
kZ(a,b){var s=Object.getPrototypeOf(a)
Object.defineProperty(s,v.dispatchPropertyName,{value:J.jq(b,s,null,null),enumerable:false,writable:true,configurable:true})
return b},
hn(a){return J.jq(a,!1,null,!!a.$iab)},
ob(a,b,c){var s=b.prototype
if(v.leafTags[a]===true)return A.hn(s)
else return J.jq(s,c,null,null)},
nN(){if(!0===$.jj)return
$.jj=!0
A.nO()},
nO(){var s,r,q,p,o,n,m,l
$.fT=Object.create(null)
$.h0=Object.create(null)
A.nM()
s=v.interceptorsByTag
r=Object.getOwnPropertyNames(s)
if(typeof window!="undefined"){window
q=function(){}
for(p=0;p<r.length;++p){o=r[p]
n=$.l0.$1(o)
if(n!=null){m=A.ob(o,s[o],n)
if(m!=null){Object.defineProperty(n,v.dispatchPropertyName,{value:m,enumerable:false,writable:true,configurable:true})
q.prototype=n}}}}for(p=0;p<r.length;++p){o=r[p]
if(/^[A-Za-z_]/.test(o)){l=s[o]
s["!"+o]=l
s["~"+o]=l
s["-"+o]=l
s["+"+o]=l
s["*"+o]=l}}},
nM(){var s,r,q,p,o,n,m=B.A()
m=A.bI(B.B,A.bI(B.C,A.bI(B.w,A.bI(B.w,A.bI(B.D,A.bI(B.E,A.bI(B.F(B.v),m)))))))
if(typeof dartNativeDispatchHooksTransformer!="undefined"){s=dartNativeDispatchHooksTransformer
if(typeof s=="function")s=[s]
if(Array.isArray(s))for(r=0;r<s.length;++r){q=s[r]
if(typeof q=="function")m=q(m)||m}}p=m.getTag
o=m.getUnknownTag
n=m.prototypeForTag
$.kP=new A.fY(p)
$.kE=new A.fZ(o)
$.l0=new A.h_(n)},
bI(a,b){return a(b)||b},
me(a,b){var s,r
for(s=0;s<a.length;++s){r=a[s]
if(!(s<b.length))return A.a(b,s)
if(!J.S(r,b[s]))return!1}return!0},
nr(a,b){var s=b.length,r=v.rttc[""+s+";"+a]
if(r==null)return null
if(s===0)return r
if(s===r.length)return r.apply(null,b)
return r(b)},
jN(a,b,c,d,e,f){var s=b?"m":"",r=c?"":"i",q=d?"u":"",p=e?"s":"",o=function(g,h){try{return new RegExp(g,h)}catch(n){return n}}(a,s+r+q+p+f)
if(o instanceof RegExp)return o
throw A.f(A.aE("Illegal RegExp pattern ("+String(o)+")",a,null))},
ou(a,b,c){var s=a.indexOf(b,c)
return s>=0},
kN(a){if(a.indexOf("$",0)>=0)return a.replace(/\$/g,"$$$$")
return a},
l1(a){if(/[[\]{}()*+?.\\^$|]/.test(a))return a.replace(/[[\]{}()*+?.\\^$|]/g,"\\$&")
return a},
a3(a,b,c){var s
if(typeof b=="string")return A.ow(a,b,c)
if(b instanceof A.c5){s=b.gba()
s.lastIndex=0
return a.replace(s,A.kN(c))}return A.ov(a,b,c)},
ov(a,b,c){var s,r,q,p
for(s=J.lo(b,a),s=s.gt(s),r=0,q="";s.l();){p=s.gp()
q=q+a.substring(r,p.gb_())+c
r=p.gaN()}s=q+a.substring(r)
return s.charCodeAt(0)==0?s:s},
ow(a,b,c){var s,r,q
if(b===""){if(a==="")return c
s=a.length
for(r=c,q=0;q<s;++q)r=r+a[q]+c
return r.charCodeAt(0)==0?r:r}if(a.indexOf(b,0)<0)return a
if(a.length<500||c.indexOf("$",0)>=0)return a.split(b).join(c)
return a.replace(new RegExp(A.l1(b),"g"),A.kN(c))},
i:function i(a,b){this.a=a
this.b=b},
cw:function cw(a,b){this.a=a
this.b=b},
bE:function bE(a,b,c){this.a=a
this.b=b
this.c=c},
cx:function cx(a){this.a=a},
cy:function cy(a){this.a=a},
cr:function cr(a,b,c){var _=this
_.a=a
_.b=b
_.c=0
_.d=null
_.$ti=c},
bW:function bW(){},
bX:function bX(a,b,c){this.a=a
this.b=b
this.$ti=c},
cj:function cj(){},
eI:function eI(a,b,c,d,e,f){var _=this
_.a=a
_.b=b
_.c=c
_.d=d
_.e=e
_.f=f},
cf:function cf(){},
dd:function dd(a,b,c){this.a=a
this.b=b
this.c=c},
dC:function dC(a){this.a=a},
eo:function eo(a){this.a=a},
c1:function c1(a,b){this.a=a
this.b=b},
cA:function cA(a){this.a=a
this.b=null},
aR:function aR(){},
d3:function d3(){},
d4:function d4(){},
dz:function dz(){},
dw:function dw(){},
bu:function bu(a,b){this.a=a
this.b=b},
du:function du(a){this.a=a},
b6:function b6(a){var _=this
_.a=0
_.f=_.e=_.d=_.c=_.b=null
_.r=0
_.$ti=a},
ef:function ef(a,b){this.a=a
this.b=b
this.c=null},
c7:function c7(a,b){this.a=a
this.$ti=b},
b8:function b8(a,b,c,d){var _=this
_.a=a
_.b=b
_.c=c
_.d=null
_.$ti=d},
c8:function c8(a,b){this.a=a
this.$ti=b},
aG:function aG(a,b,c,d){var _=this
_.a=a
_.b=b
_.c=c
_.d=null
_.$ti=d},
N:function N(a,b){this.a=a
this.$ti=b},
b7:function b7(a,b,c,d){var _=this
_.a=a
_.b=b
_.c=c
_.d=null
_.$ti=d},
fY:function fY(a){this.a=a},
fZ:function fZ(a){this.a=a},
h_:function h_(a){this.a=a},
ag:function ag(){},
bj:function bj(){},
bD:function bD(){},
bk:function bk(){},
c5:function c5(a,b){var _=this
_.a=a
_.b=b
_.e=_.d=_.c=null},
dL:function dL(a){this.b=a},
dD:function dD(a,b,c){this.a=a
this.b=b
this.c=c},
dE:function dE(a,b,c){var _=this
_.a=a
_.b=b
_.c=c
_.d=null},
dy:function dy(a,b){this.a=a
this.c=b},
dO:function dO(a,b,c){this.a=a
this.b=b
this.c=c},
dP:function dP(a,b,c){var _=this
_.a=a
_.b=b
_.c=c
_.d=null},
z(a){var s,r
if(t.e.b(a))return a
s=A.aU(a.length,null,!1,t.z)
for(r=0;r<a.length;++r)B.b.q(s,r,a[r])
return s},
lP(a){return new Int8Array(a)},
el(a){return new Uint8Array(a)},
jU(a,b,c){return c==null?new Uint8Array(a,b):new Uint8Array(a,b,c)},
mv(a,b,c){if(a>>>0!==a||a>=c)throw A.f(A.fS(b,a))},
km(a,b,c){var s
if(!(a>>>0!==a))s=b>>>0!==b||a>b||b>c
else s=!0
if(s)throw A.f(A.nt(a,b,c))
return b},
aV:function aV(){},
bz:function bz(){},
ce:function ce(){},
f8:function f8(a){this.a=a},
dh:function dh(){},
X:function X(){},
cd:function cd(){},
af:function af(){},
di:function di(){},
dj:function dj(){},
dk:function dk(){},
dl:function dl(){},
dm:function dm(){},
dn:function dn(){},
dp:function dp(){},
b9:function b9(){},
ba:function ba(){},
cs:function cs(){},
ct:function ct(){},
cu:function cu(){},
cv:function cv(){},
iR(a,b){var s=b.c
return s==null?b.c=A.cD(a,"b5",[b.x]):s},
k_(a){var s=a.w
if(s===6||s===7)return A.k_(a.x)
return s===11||s===12},
lU(a){return a.as},
kW(a,b){var s,r=b.length
for(s=0;s<r;++s)if(!a[s].b(b[s]))return!1
return!0},
aP(a){return A.f7(v.typeUniverse,a,!1)},
bl(a1,a2,a3,a4){var s,r,q,p,o,n,m,l,k,j,i,h,g,f,e,d,c,b,a,a0=a2.w
switch(a0){case 5:case 1:case 2:case 3:case 4:return a2
case 6:s=a2.x
r=A.bl(a1,s,a3,a4)
if(r===s)return a2
return A.ke(a1,r,!0)
case 7:s=a2.x
r=A.bl(a1,s,a3,a4)
if(r===s)return a2
return A.kd(a1,r,!0)
case 8:q=a2.y
p=A.bH(a1,q,a3,a4)
if(p===q)return a2
return A.cD(a1,a2.x,p)
case 9:o=a2.x
n=A.bl(a1,o,a3,a4)
m=a2.y
l=A.bH(a1,m,a3,a4)
if(n===o&&l===m)return a2
return A.iV(a1,n,l)
case 10:k=a2.x
j=a2.y
i=A.bH(a1,j,a3,a4)
if(i===j)return a2
return A.kf(a1,k,i)
case 11:h=a2.x
g=A.bl(a1,h,a3,a4)
f=a2.y
e=A.nc(a1,f,a3,a4)
if(g===h&&e===f)return a2
return A.kc(a1,g,e)
case 12:d=a2.y
a4+=d.length
c=A.bH(a1,d,a3,a4)
o=a2.x
n=A.bl(a1,o,a3,a4)
if(c===d&&n===o)return a2
return A.iW(a1,n,c,!0)
case 13:b=a2.x
if(b<a4)return a2
a=a3[b-a4]
if(a==null)return a2
return a
default:throw A.f(A.d1("Attempted to substitute unexpected RTI kind "+a0))}},
bH(a,b,c,d){var s,r,q,p,o=b.length,n=A.fa(o)
for(s=!1,r=0;r<o;++r){q=b[r]
p=A.bl(a,q,c,d)
if(p!==q)s=!0
n[r]=p}return s?n:b},
nd(a,b,c,d){var s,r,q,p,o,n,m=b.length,l=A.fa(m)
for(s=!1,r=0;r<m;r+=3){q=b[r]
p=b[r+1]
o=b[r+2]
n=A.bl(a,o,c,d)
if(n!==o)s=!0
l.splice(r,3,q,p,n)}return s?l:b},
nc(a,b,c,d){var s,r=b.a,q=A.bH(a,r,c,d),p=b.b,o=A.bH(a,p,c,d),n=b.c,m=A.nd(a,n,c,d)
if(q===r&&o===p&&m===n)return b
s=new A.dJ()
s.a=q
s.b=o
s.c=m
return s},
d(a,b){a[v.arrayRti]=b
return a},
kH(a){var s=a.$S
if(s!=null){if(typeof s=="number")return A.nH(s)
return a.$S()}return null},
nP(a,b){var s
if(A.k_(b))if(a instanceof A.aR){s=A.kH(a)
if(s!=null)return s}return A.a6(a)},
a6(a){if(a instanceof A.x)return A.A(a)
if(Array.isArray(a))return A.Q(a)
return A.j3(J.bn(a))},
Q(a){var s=a[v.arrayRti],r=t.b
if(s==null)return r
if(s.constructor!==r.constructor)return r
return s},
A(a){var s=a.$ti
return s!=null?s:A.j3(a)},
j3(a){var s=a.constructor,r=s.$ccache
if(r!=null)return r
return A.mG(a,s)},
mG(a,b){var s=a instanceof A.aR?Object.getPrototypeOf(Object.getPrototypeOf(a)).constructor:b,r=A.mn(v.typeUniverse,s.name)
b.$ccache=r
return r},
nH(a){var s,r=v.types,q=r[a]
if(typeof q=="string"){s=A.f7(v.typeUniverse,q,!1)
r[a]=s
return s}return q},
nG(a){return A.bm(A.A(a))},
jb(a){var s
if(a instanceof A.ag)return A.nu(a.$r,a.af())
s=a instanceof A.aR?A.kH(a):null
if(s!=null)return s
if(t.bW.b(a))return J.lq(a).a
if(Array.isArray(a))return A.Q(a)
return A.a6(a)},
bm(a){var s=a.r
return s==null?a.r=new A.f6(a):s},
nu(a,b){var s,r,q=b,p=q.length
if(p===0)return t.cD
if(0>=p)return A.a(q,0)
s=A.cF(v.typeUniverse,A.jb(q[0]),"@<0>")
for(r=1;r<p;++r){if(!(r<q.length))return A.a(q,r)
s=A.kg(v.typeUniverse,s,A.jb(q[r]))}return A.cF(v.typeUniverse,s,a)},
av(a){return A.bm(A.f7(v.typeUniverse,a,!1))},
mF(a){var s=this
s.b=A.na(s)
return s.b(a)},
na(a){var s,r,q,p,o
if(a===t.K)return A.mQ
if(A.bo(a))return A.mU
s=a.w
if(s===6)return A.mD
if(s===1)return A.ku
if(s===7)return A.mL
r=A.n9(a)
if(r!=null)return r
if(s===8){q=a.x
if(a.y.every(A.bo)){a.f="$i"+q
if(q==="l")return A.mO
if(a===t.m)return A.mN
return A.mT}}else if(s===10){p=A.nr(a.x,a.y)
o=p==null?A.ku:p
return o==null?A.bF(o):o}return A.mB},
n9(a){if(a.w===8){if(a===t.S)return A.ks
if(a===t.i||a===t.o)return A.mP
if(a===t.N)return A.mS
if(a===t.y)return A.j4}return null},
mE(a){var s=this,r=A.mA
if(A.bo(s))r=A.ms
else if(s===t.K)r=A.bF
else if(A.bS(s)){r=A.mC
if(s===t.a3)r=A.mr
else if(s===t.aD)r=A.iX
else if(s===t.cG)r=A.mp
else if(s===t.ae)r=A.kk
else if(s===t.dd)r=A.mq
else if(s===t.aQ)r=A.q}else if(s===t.S)r=A.j
else if(s===t.N)r=A.B
else if(s===t.y)r=A.F
else if(s===t.o)r=A.kj
else if(s===t.i)r=A.a_
else if(s===t.m)r=A.c
s.a=r
return s.a(a)},
mB(a){var s=this
if(a==null)return A.bS(s)
return A.nQ(v.typeUniverse,A.nP(a,s),s)},
mD(a){if(a==null)return!0
return this.x.b(a)},
mT(a){var s,r=this
if(a==null)return A.bS(r)
s=r.f
if(a instanceof A.x)return!!a[s]
return!!J.bn(a)[s]},
mO(a){var s,r=this
if(a==null)return A.bS(r)
if(typeof a!="object")return!1
if(Array.isArray(a))return!0
s=r.f
if(a instanceof A.x)return!!a[s]
return!!J.bn(a)[s]},
mN(a){var s=this
if(a==null)return!1
if(typeof a=="object"){if(a instanceof A.x)return!!a[s.f]
return!0}if(typeof a=="function")return!0
return!1},
kt(a){if(typeof a=="object"){if(a instanceof A.x)return t.m.b(a)
return!0}if(typeof a=="function")return!0
return!1},
mA(a){var s=this
if(a==null){if(A.bS(s))return a}else if(s.b(a))return a
throw A.U(A.kn(a,s),new Error())},
mC(a){var s=this
if(a==null||s.b(a))return a
throw A.U(A.kn(a,s),new Error())},
kn(a,b){return new A.cB("TypeError: "+A.k5(a,A.ah(b,null)))},
k5(a,b){return A.eb(a)+": type '"+A.ah(A.jb(a),null)+"' is not a subtype of type '"+b+"'"},
am(a,b){return new A.cB("TypeError: "+A.k5(a,b))},
mL(a){var s=this
return s.x.b(a)||A.iR(v.typeUniverse,s).b(a)},
mQ(a){return a!=null},
bF(a){if(a!=null)return a
throw A.U(A.am(a,"Object"),new Error())},
mU(a){return!0},
ms(a){return a},
ku(a){return!1},
j4(a){return!0===a||!1===a},
F(a){if(!0===a)return!0
if(!1===a)return!1
throw A.U(A.am(a,"bool"),new Error())},
mp(a){if(!0===a)return!0
if(!1===a)return!1
if(a==null)return a
throw A.U(A.am(a,"bool?"),new Error())},
a_(a){if(typeof a=="number")return a
throw A.U(A.am(a,"double"),new Error())},
mq(a){if(typeof a=="number")return a
if(a==null)return a
throw A.U(A.am(a,"double?"),new Error())},
ks(a){return typeof a=="number"&&Math.floor(a)===a},
j(a){if(typeof a=="number"&&Math.floor(a)===a)return a
throw A.U(A.am(a,"int"),new Error())},
mr(a){if(typeof a=="number"&&Math.floor(a)===a)return a
if(a==null)return a
throw A.U(A.am(a,"int?"),new Error())},
mP(a){return typeof a=="number"},
kj(a){if(typeof a=="number")return a
throw A.U(A.am(a,"num"),new Error())},
kk(a){if(typeof a=="number")return a
if(a==null)return a
throw A.U(A.am(a,"num?"),new Error())},
mS(a){return typeof a=="string"},
B(a){if(typeof a=="string")return a
throw A.U(A.am(a,"String"),new Error())},
iX(a){if(typeof a=="string")return a
if(a==null)return a
throw A.U(A.am(a,"String?"),new Error())},
c(a){if(A.kt(a))return a
throw A.U(A.am(a,"JSObject"),new Error())},
q(a){if(a==null)return a
if(A.kt(a))return a
throw A.U(A.am(a,"JSObject?"),new Error())},
ky(a,b){var s,r,q
for(s="",r="",q=0;q<a.length;++q,r=", ")s+=r+A.ah(a[q],b)
return s},
n2(a,b){var s,r,q,p,o,n,m=a.x,l=a.y
if(""===m)return"("+A.ky(l,b)+")"
s=l.length
r=m.split(",")
q=r.length-s
for(p="(",o="",n=0;n<s;++n,o=", "){p+=o
if(q===0)p+="{"
p+=A.ah(l[n],b)
if(q>=0)p+=" "+r[q];++q}return p+"})"},
kp(a3,a4,a5){var s,r,q,p,o,n,m,l,k,j,i,h,g,f,e,d,c,b,a,a0,a1=", ",a2=null
if(a5!=null){s=a5.length
if(a4==null)a4=A.d([],t.s)
else a2=a4.length
r=a4.length
for(q=s;q>0;--q)B.b.i(a4,"T"+(r+q))
for(p=t.X,o="<",n="",q=0;q<s;++q,n=a1){m=a4.length
l=m-1-q
if(!(l>=0))return A.a(a4,l)
o=o+n+a4[l]
k=a5[q]
j=k.w
if(!(j===2||j===3||j===4||j===5||k===p))o+=" extends "+A.ah(k,a4)}o+=">"}else o=""
p=a3.x
i=a3.y
h=i.a
g=h.length
f=i.b
e=f.length
d=i.c
c=d.length
b=A.ah(p,a4)
for(a="",a0="",q=0;q<g;++q,a0=a1)a+=a0+A.ah(h[q],a4)
if(e>0){a+=a0+"["
for(a0="",q=0;q<e;++q,a0=a1)a+=a0+A.ah(f[q],a4)
a+="]"}if(c>0){a+=a0+"{"
for(a0="",q=0;q<c;q+=3,a0=a1){a+=a0
if(d[q+1])a+="required "
a+=A.ah(d[q+2],a4)+" "+d[q]}a+="}"}if(a2!=null){a4.toString
a4.length=a2}return o+"("+a+") => "+b},
ah(a,b){var s,r,q,p,o,n,m,l=a.w
if(l===5)return"erased"
if(l===2)return"dynamic"
if(l===3)return"void"
if(l===1)return"Never"
if(l===4)return"any"
if(l===6){s=a.x
r=A.ah(s,b)
q=s.w
return(q===11||q===12?"("+r+")":r)+"?"}if(l===7)return"FutureOr<"+A.ah(a.x,b)+">"
if(l===8){p=A.nf(a.x)
o=a.y
return o.length>0?p+("<"+A.ky(o,b)+">"):p}if(l===10)return A.n2(a,b)
if(l===11)return A.kp(a,b,null)
if(l===12)return A.kp(a.x,b,a.y)
if(l===13){n=a.x
m=b.length
n=m-1-n
if(!(n>=0&&n<m))return A.a(b,n)
return b[n]}return"?"},
nf(a){var s=v.mangledGlobalNames[a]
if(s!=null)return s
return"minified:"+a},
mo(a,b){var s=a.tR[b]
while(typeof s=="string")s=a.tR[s]
return s},
mn(a,b){var s,r,q,p,o,n=a.eT,m=n[b]
if(m==null)return A.f7(a,b,!1)
else if(typeof m=="number"){s=m
r=A.cE(a,5,"#")
q=A.fa(s)
for(p=0;p<s;++p)q[p]=r
o=A.cD(a,b,q)
n[b]=o
return o}else return m},
mm(a,b){return A.kh(a.tR,b)},
ml(a,b){return A.kh(a.eT,b)},
f7(a,b,c){var s,r=a.eC,q=r.get(b)
if(q!=null)return q
s=A.ka(A.k8(a,null,b,!1))
r.set(b,s)
return s},
cF(a,b,c){var s,r,q=b.z
if(q==null)q=b.z=new Map()
s=q.get(c)
if(s!=null)return s
r=A.ka(A.k8(a,b,c,!0))
q.set(c,r)
return r},
kg(a,b,c){var s,r,q,p=b.Q
if(p==null)p=b.Q=new Map()
s=c.as
r=p.get(s)
if(r!=null)return r
q=A.iV(a,b,c.w===9?c.y:[c])
p.set(s,q)
return q},
aY(a,b){b.a=A.mE
b.b=A.mF
return b},
cE(a,b,c){var s,r,q=a.eC.get(c)
if(q!=null)return q
s=new A.as(null,null)
s.w=b
s.as=c
r=A.aY(a,s)
a.eC.set(c,r)
return r},
ke(a,b,c){var s,r=b.as+"?",q=a.eC.get(r)
if(q!=null)return q
s=A.mj(a,b,r,c)
a.eC.set(r,s)
return s},
mj(a,b,c,d){var s,r,q
if(d){s=b.w
r=!0
if(!A.bo(b))if(!(b===t.P||b===t.T))if(s!==6)r=s===7&&A.bS(b.x)
if(r)return b
else if(s===1)return t.P}q=new A.as(null,null)
q.w=6
q.x=b
q.as=c
return A.aY(a,q)},
kd(a,b,c){var s,r=b.as+"/",q=a.eC.get(r)
if(q!=null)return q
s=A.mh(a,b,r,c)
a.eC.set(r,s)
return s},
mh(a,b,c,d){var s,r
if(d){s=b.w
if(A.bo(b)||b===t.K)return b
else if(s===1)return A.cD(a,"b5",[b])
else if(b===t.P||b===t.T)return t.bc}r=new A.as(null,null)
r.w=7
r.x=b
r.as=c
return A.aY(a,r)},
mk(a,b){var s,r,q=""+b+"^",p=a.eC.get(q)
if(p!=null)return p
s=new A.as(null,null)
s.w=13
s.x=b
s.as=q
r=A.aY(a,s)
a.eC.set(q,r)
return r},
cC(a){var s,r,q,p=a.length
for(s="",r="",q=0;q<p;++q,r=",")s+=r+a[q].as
return s},
mg(a){var s,r,q,p,o,n=a.length
for(s="",r="",q=0;q<n;q+=3,r=","){p=a[q]
o=a[q+1]?"!":":"
s+=r+p+o+a[q+2].as}return s},
cD(a,b,c){var s,r,q,p=b
if(c.length>0)p+="<"+A.cC(c)+">"
s=a.eC.get(p)
if(s!=null)return s
r=new A.as(null,null)
r.w=8
r.x=b
r.y=c
if(c.length>0)r.c=c[0]
r.as=p
q=A.aY(a,r)
a.eC.set(p,q)
return q},
iV(a,b,c){var s,r,q,p,o,n
if(b.w===9){s=b.x
r=b.y.concat(c)}else{r=c
s=b}q=s.as+(";<"+A.cC(r)+">")
p=a.eC.get(q)
if(p!=null)return p
o=new A.as(null,null)
o.w=9
o.x=s
o.y=r
o.as=q
n=A.aY(a,o)
a.eC.set(q,n)
return n},
kf(a,b,c){var s,r,q="+"+(b+"("+A.cC(c)+")"),p=a.eC.get(q)
if(p!=null)return p
s=new A.as(null,null)
s.w=10
s.x=b
s.y=c
s.as=q
r=A.aY(a,s)
a.eC.set(q,r)
return r},
kc(a,b,c){var s,r,q,p,o,n=b.as,m=c.a,l=m.length,k=c.b,j=k.length,i=c.c,h=i.length,g="("+A.cC(m)
if(j>0){s=l>0?",":""
g+=s+"["+A.cC(k)+"]"}if(h>0){s=l>0?",":""
g+=s+"{"+A.mg(i)+"}"}r=n+(g+")")
q=a.eC.get(r)
if(q!=null)return q
p=new A.as(null,null)
p.w=11
p.x=b
p.y=c
p.as=r
o=A.aY(a,p)
a.eC.set(r,o)
return o},
iW(a,b,c,d){var s,r=b.as+("<"+A.cC(c)+">"),q=a.eC.get(r)
if(q!=null)return q
s=A.mi(a,b,c,r,d)
a.eC.set(r,s)
return s},
mi(a,b,c,d,e){var s,r,q,p,o,n,m,l
if(e){s=c.length
r=A.fa(s)
for(q=0,p=0;p<s;++p){o=c[p]
if(o.w===1){r[p]=o;++q}}if(q>0){n=A.bl(a,b,r,0)
m=A.bH(a,c,r,0)
return A.iW(a,n,m,c!==m)}}l=new A.as(null,null)
l.w=12
l.x=b
l.y=c
l.as=d
return A.aY(a,l)},
k8(a,b,c,d){return{u:a,e:b,r:c,s:[],p:0,n:d}},
ka(a){var s,r,q,p,o,n,m,l=a.r,k=a.s
for(s=l.length,r=0;r<s;){q=l.charCodeAt(r)
if(q>=48&&q<=57)r=A.m9(r+1,q,l,k)
else if((((q|32)>>>0)-97&65535)<26||q===95||q===36||q===124)r=A.k9(a,r,l,k,!1)
else if(q===46)r=A.k9(a,r,l,k,!0)
else{++r
switch(q){case 44:break
case 58:k.push(!1)
break
case 33:k.push(!0)
break
case 59:k.push(A.bi(a.u,a.e,k.pop()))
break
case 94:k.push(A.mk(a.u,k.pop()))
break
case 35:k.push(A.cE(a.u,5,"#"))
break
case 64:k.push(A.cE(a.u,2,"@"))
break
case 126:k.push(A.cE(a.u,3,"~"))
break
case 60:k.push(a.p)
a.p=k.length
break
case 62:A.mb(a,k)
break
case 38:A.ma(a,k)
break
case 63:p=a.u
k.push(A.ke(p,A.bi(p,a.e,k.pop()),a.n))
break
case 47:p=a.u
k.push(A.kd(p,A.bi(p,a.e,k.pop()),a.n))
break
case 40:k.push(-3)
k.push(a.p)
a.p=k.length
break
case 41:A.m8(a,k)
break
case 91:k.push(a.p)
a.p=k.length
break
case 93:o=k.splice(a.p)
A.kb(a.u,a.e,o)
a.p=k.pop()
k.push(o)
k.push(-1)
break
case 123:k.push(a.p)
a.p=k.length
break
case 125:o=k.splice(a.p)
A.md(a.u,a.e,o)
a.p=k.pop()
k.push(o)
k.push(-2)
break
case 43:n=l.indexOf("(",r)
k.push(l.substring(r,n))
k.push(-4)
k.push(a.p)
a.p=k.length
r=n+1
break
default:throw"Bad character "+q}}}m=k.pop()
return A.bi(a.u,a.e,m)},
m9(a,b,c,d){var s,r,q=b-48
for(s=c.length;a<s;++a){r=c.charCodeAt(a)
if(!(r>=48&&r<=57))break
q=q*10+(r-48)}d.push(q)
return a},
k9(a,b,c,d,e){var s,r,q,p,o,n,m=b+1
for(s=c.length;m<s;++m){r=c.charCodeAt(m)
if(r===46){if(e)break
e=!0}else{if(!((((r|32)>>>0)-97&65535)<26||r===95||r===36||r===124))q=r>=48&&r<=57
else q=!0
if(!q)break}}p=c.substring(b,m)
if(e){s=a.u
o=a.e
if(o.w===9)o=o.x
n=A.mo(s,o.x)[p]
if(n==null)A.M('No "'+p+'" in "'+A.lU(o)+'"')
d.push(A.cF(s,o,n))}else d.push(p)
return m},
mb(a,b){var s,r=a.u,q=A.k7(a,b),p=b.pop()
if(typeof p=="string")b.push(A.cD(r,p,q))
else{s=A.bi(r,a.e,p)
switch(s.w){case 11:b.push(A.iW(r,s,q,a.n))
break
default:b.push(A.iV(r,s,q))
break}}},
m8(a,b){var s,r,q,p=a.u,o=b.pop(),n=null,m=null
if(typeof o=="number")switch(o){case-1:n=b.pop()
break
case-2:m=b.pop()
break
default:b.push(o)
break}else b.push(o)
s=A.k7(a,b)
o=b.pop()
switch(o){case-3:o=b.pop()
if(n==null)n=p.sEA
if(m==null)m=p.sEA
r=A.bi(p,a.e,o)
q=new A.dJ()
q.a=s
q.b=n
q.c=m
b.push(A.kc(p,r,q))
return
case-4:b.push(A.kf(p,b.pop(),s))
return
default:throw A.f(A.d1("Unexpected state under `()`: "+A.k(o)))}},
ma(a,b){var s=b.pop()
if(0===s){b.push(A.cE(a.u,1,"0&"))
return}if(1===s){b.push(A.cE(a.u,4,"1&"))
return}throw A.f(A.d1("Unexpected extended operation "+A.k(s)))},
k7(a,b){var s=b.splice(a.p)
A.kb(a.u,a.e,s)
a.p=b.pop()
return s},
bi(a,b,c){if(typeof c=="string")return A.cD(a,c,a.sEA)
else if(typeof c=="number"){b.toString
return A.mc(a,b,c)}else return c},
kb(a,b,c){var s,r=c.length
for(s=0;s<r;++s)c[s]=A.bi(a,b,c[s])},
md(a,b,c){var s,r=c.length
for(s=2;s<r;s+=3)c[s]=A.bi(a,b,c[s])},
mc(a,b,c){var s,r,q=b.w
if(q===9){if(c===0)return b.x
s=b.y
r=s.length
if(c<=r)return s[c-1]
c-=r
b=b.x
q=b.w}else if(c===0)return b
if(q!==8)throw A.f(A.d1("Indexed base must be an interface type"))
s=b.y
if(c<=s.length)return s[c-1]
throw A.f(A.d1("Bad index "+c+" for "+b.j(0)))},
nQ(a,b,c){var s,r=b.d
if(r==null)r=b.d=new Map()
s=r.get(c)
if(s==null){s=A.R(a,b,null,c,null)
r.set(c,s)}return s},
R(a,b,c,d,e){var s,r,q,p,o,n,m,l,k,j,i
if(b===d)return!0
if(A.bo(d))return!0
s=b.w
if(s===4)return!0
if(A.bo(b))return!1
if(b.w===1)return!0
r=s===13
if(r)if(A.R(a,c[b.x],c,d,e))return!0
q=d.w
p=t.P
if(b===p||b===t.T){if(q===7)return A.R(a,b,c,d.x,e)
return d===p||d===t.T||q===6}if(d===t.K){if(s===7)return A.R(a,b.x,c,d,e)
return s!==6}if(s===7){if(!A.R(a,b.x,c,d,e))return!1
return A.R(a,A.iR(a,b),c,d,e)}if(s===6)return A.R(a,p,c,d,e)&&A.R(a,b.x,c,d,e)
if(q===7){if(A.R(a,b,c,d.x,e))return!0
return A.R(a,b,c,A.iR(a,d),e)}if(q===6)return A.R(a,b,c,p,e)||A.R(a,b,c,d.x,e)
if(r)return!1
p=s!==11
if((!p||s===12)&&d===t.Z)return!0
o=s===10
if(o&&d===t.cY)return!0
if(q===12){if(b===t.g)return!0
if(s!==12)return!1
n=b.y
m=d.y
l=n.length
if(l!==m.length)return!1
c=c==null?n:n.concat(c)
e=e==null?m:m.concat(e)
for(k=0;k<l;++k){j=n[k]
i=m[k]
if(!A.R(a,j,c,i,e)||!A.R(a,i,e,j,c))return!1}return A.kr(a,b.x,c,d.x,e)}if(q===11){if(b===t.g)return!0
if(p)return!1
return A.kr(a,b,c,d,e)}if(s===8){if(q!==8)return!1
return A.mM(a,b,c,d,e)}if(o&&q===10)return A.mR(a,b,c,d,e)
return!1},
kr(a3,a4,a5,a6,a7){var s,r,q,p,o,n,m,l,k,j,i,h,g,f,e,d,c,b,a,a0,a1,a2
if(!A.R(a3,a4.x,a5,a6.x,a7))return!1
s=a4.y
r=a6.y
q=s.a
p=r.a
o=q.length
n=p.length
if(o>n)return!1
m=n-o
l=s.b
k=r.b
j=l.length
i=k.length
if(o+j<n+i)return!1
for(h=0;h<o;++h){g=q[h]
if(!A.R(a3,p[h],a7,g,a5))return!1}for(h=0;h<m;++h){g=l[h]
if(!A.R(a3,p[o+h],a7,g,a5))return!1}for(h=0;h<i;++h){g=l[m+h]
if(!A.R(a3,k[h],a7,g,a5))return!1}f=s.c
e=r.c
d=f.length
c=e.length
for(b=0,a=0;a<c;a+=3){a0=e[a]
for(;;){if(b>=d)return!1
a1=f[b]
b+=3
if(a0<a1)return!1
a2=f[b-2]
if(a1<a0){if(a2)return!1
continue}g=e[a+1]
if(a2&&!g)return!1
g=f[b-1]
if(!A.R(a3,e[a+2],a7,g,a5))return!1
break}}while(b<d){if(f[b+1])return!1
b+=3}return!0},
mM(a,b,c,d,e){var s,r,q,p,o,n=b.x,m=d.x
while(n!==m){s=a.tR[n]
if(s==null)return!1
if(typeof s=="string"){n=s
continue}r=s[m]
if(r==null)return!1
q=r.length
p=q>0?new Array(q):v.typeUniverse.sEA
for(o=0;o<q;++o)p[o]=A.cF(a,b,r[o])
return A.ki(a,p,null,c,d.y,e)}return A.ki(a,b.y,null,c,d.y,e)},
ki(a,b,c,d,e,f){var s,r=b.length
for(s=0;s<r;++s)if(!A.R(a,b[s],d,e[s],f))return!1
return!0},
mR(a,b,c,d,e){var s,r=b.y,q=d.y,p=r.length
if(p!==q.length)return!1
if(b.x!==d.x)return!1
for(s=0;s<p;++s)if(!A.R(a,r[s],c,q[s],e))return!1
return!0},
bS(a){var s=a.w,r=!0
if(!(a===t.P||a===t.T))if(!A.bo(a))if(s!==6)r=s===7&&A.bS(a.x)
return r},
bo(a){var s=a.w
return s===2||s===3||s===4||s===5||a===t.X},
kh(a,b){var s,r,q=Object.keys(b),p=q.length
for(s=0;s<p;++s){r=q[s]
a[r]=b[r]}},
fa(a){return a>0?new Array(a):v.typeUniverse.sEA},
as:function as(a,b){var _=this
_.a=a
_.b=b
_.r=_.f=_.d=_.c=null
_.w=0
_.as=_.Q=_.z=_.y=_.x=null},
dJ:function dJ(){this.c=this.b=this.a=null},
f6:function f6(a){this.a=a},
dI:function dI(){},
cB:function cB(a){this.a=a},
m0(){var s,r,q
if(self.scheduleImmediate!=null)return A.nl()
if(self.MutationObserver!=null&&self.document!=null){s={}
r=self.document.createElement("div")
q=self.document.createElement("span")
s.a=null
new self.MutationObserver(A.bO(new A.eM(s),1)).observe(r,{childList:true})
return new A.eL(s,r,q)}else if(self.setImmediate!=null)return A.nm()
return A.nn()},
m1(a){self.scheduleImmediate(A.bO(new A.eN(t.M.a(a)),0))},
m2(a){self.setImmediate(A.bO(new A.eO(t.M.a(a)),0))},
m3(a){t.M.a(a)
A.mf(0,a)},
mf(a,b){var s=new A.f4()
s.bH(a,b)
return s},
j6(a){return new A.dF(new A.T($.K,a.h("T<0>")),a.h("dF<0>"))},
j0(a,b){a.$2(0,null)
b.b=!0
return b.a},
iY(a,b){A.mt(a,b)},
j_(a,b){b.aJ(a)},
iZ(a,b){b.aK(A.aQ(a),A.bR(a))},
mt(a,b){var s,r,q=new A.fc(b),p=new A.fd(b)
if(a instanceof A.T)a.bh(q,p,t.z)
else{s=t.z
if(a instanceof A.T)a.by(q,p,s)
else{r=new A.T($.K,t._)
r.a=8
r.c=a
r.bh(q,p,s)}}},
jd(a){var s=function(b,c){return function(d,e){while(true){try{b(d,e)
break}catch(r){e=r
d=c}}}}(a,1)
return $.K.bv(new A.fG(s),t.H,t.S,t.z)},
iI(a){var s
if(t.C.b(a)){s=a.ga4()
if(s!=null)return s}return B.n},
mH(a,b){if($.K===B.i)return null
return null},
mI(a,b){if($.K!==B.i)A.mH(a,b)
if(b==null)if(t.C.b(a)){b=a.ga4()
if(b==null){A.jY(a,B.n)
b=B.n}}else b=B.n
else if(t.C.b(a))A.jY(a,b)
return new A.ak(a,b)},
iT(a,b,c){var s,r,q,p,o={},n=o.a=a
for(s=t._;r=n.a,(r&4)!==0;n=a){a=s.a(n.c)
o.a=a}if(n===b){s=A.lW()
b.av(new A.ak(new A.ap(!0,n,null,"Cannot complete a future with itself"),s))
return}q=b.a&1
s=n.a=r|q
if((s&24)===0){p=t.F.a(b.c)
b.a=b.a&1|4
b.c=n
n.bb(p)
return}if(!c)if(b.c==null)n=(s&16)===0||q!==0
else n=!1
else n=!0
if(n){p=b.ag()
b.ae(o.a)
A.bC(b,p)
return}b.a^=2
A.dR(null,null,b.b,t.M.a(new A.eV(o,b)))},
bC(a,b){var s,r,q,p,o,n,m,l,k,j,i,h,g,f,e,d={},c=d.a=a
for(s=t.u,r=t.F;;){q={}
p=c.a
o=(p&16)===0
n=!o
if(b==null){if(n&&(p&1)===0){m=s.a(c.c)
A.j8(m.a,m.b)}return}q.a=b
l=b.a
for(c=b;l!=null;c=l,l=k){c.a=null
A.bC(d.a,c)
q.a=l
k=l.a}p=d.a
j=p.c
q.b=n
q.c=j
if(o){i=c.c
i=(i&1)!==0||(i&15)===8}else i=!0
if(i){h=c.b.b
if(n){p=p.b===h
p=!(p||p)}else p=!1
if(p){s.a(j)
A.j8(j.a,j.b)
return}g=$.K
if(g!==h)$.K=h
else g=null
c=c.c
if((c&15)===8)new A.eZ(q,d,n).$0()
else if(o){if((c&1)!==0)new A.eY(q,j).$0()}else if((c&2)!==0)new A.eX(d,q).$0()
if(g!=null)$.K=g
c=q.c
if(c instanceof A.T){p=q.a.$ti
p=p.h("b5<2>").b(c)||!p.y[1].b(c)}else p=!1
if(p){f=q.a.b
if((c.a&24)!==0){e=r.a(f.c)
f.c=null
b=f.ah(e)
f.a=c.a&30|f.a&1
f.c=c.c
d.a=c
continue}else A.iT(c,f,!0)
return}}f=q.a.b
e=r.a(f.c)
f.c=null
b=f.ah(e)
c=q.b
p=q.c
if(!c){f.$ti.c.a(p)
f.a=8
f.c=p}else{s.a(p)
f.a=f.a&1|16
f.c=p}d.a=f
c=f}},
n3(a,b){var s
if(t.V.b(a))return b.bv(a,t.z,t.K,t.l)
s=t.v
if(s.b(a))return s.a(a)
throw A.f(A.jG(a,"onError",u.c))},
mZ(){var s,r
for(s=$.bG;s!=null;s=$.bG){$.cI=null
r=s.b
$.bG=r
if(r==null)$.cH=null
s.a.$0()}},
nb(){$.j5=!0
try{A.mZ()}finally{$.cI=null
$.j5=!1
if($.bG!=null)$.jD().$1(A.kF())}},
kA(a){var s=new A.dG(a),r=$.cH
if(r==null){$.bG=$.cH=s
if(!$.j5)$.jD().$1(A.kF())}else $.cH=r.b=s},
n8(a){var s,r,q,p=$.bG
if(p==null){A.kA(a)
$.cI=$.cH
return}s=new A.dG(a)
r=$.cI
if(r==null){s.b=p
$.bG=$.cI=s}else{q=r.b
s.b=q
$.cI=r.b=s
if(q==null)$.cH=s}},
oG(a,b){A.fP(a,"stream",t.K)
return new A.dN(b.h("dN<0>"))},
j8(a,b){A.n8(new A.fA(a,b))},
kx(a,b,c,d,e){var s,r=$.K
if(r===c)return d.$0()
$.K=c
s=r
try{r=d.$0()
return r}finally{$.K=s}},
n5(a,b,c,d,e,f,g){var s,r=$.K
if(r===c)return d.$1(e)
$.K=c
s=r
try{r=d.$1(e)
return r}finally{$.K=s}},
n4(a,b,c,d,e,f,g,h,i){var s,r=$.K
if(r===c)return d.$2(e,f)
$.K=c
s=r
try{r=d.$2(e,f)
return r}finally{$.K=s}},
dR(a,b,c,d){t.M.a(d)
if(B.i!==c){d=c.c5(d)
d=d}A.kA(d)},
eM:function eM(a){this.a=a},
eL:function eL(a,b,c){this.a=a
this.b=b
this.c=c},
eN:function eN(a){this.a=a},
eO:function eO(a){this.a=a},
f4:function f4(){},
f5:function f5(a,b){this.a=a
this.b=b},
dF:function dF(a,b){this.a=a
this.b=!1
this.$ti=b},
fc:function fc(a){this.a=a},
fd:function fd(a){this.a=a},
fG:function fG(a){this.a=a},
ak:function ak(a,b){this.a=a
this.b=b},
dH:function dH(){},
cp:function cp(a,b){this.a=a
this.$ti=b},
bg:function bg(a,b,c,d,e){var _=this
_.a=null
_.b=a
_.c=b
_.d=c
_.e=d
_.$ti=e},
T:function T(a,b){var _=this
_.a=0
_.b=a
_.c=null
_.$ti=b},
eS:function eS(a,b){this.a=a
this.b=b},
eW:function eW(a,b){this.a=a
this.b=b},
eV:function eV(a,b){this.a=a
this.b=b},
eU:function eU(a,b){this.a=a
this.b=b},
eT:function eT(a,b){this.a=a
this.b=b},
eZ:function eZ(a,b,c){this.a=a
this.b=b
this.c=c},
f_:function f_(a,b){this.a=a
this.b=b},
f0:function f0(a){this.a=a},
eY:function eY(a,b){this.a=a
this.b=b},
eX:function eX(a,b){this.a=a
this.b=b},
dG:function dG(a){this.a=a
this.b=null},
dN:function dN(a){this.$ti=a},
cG:function cG(){},
dM:function dM(){},
f3:function f3(a,b){this.a=a
this.b=b},
fA:function fA(a,b){this.a=a
this.b=b},
df(a,b,c){return b.h("@<0>").F(c).h("jP<1,2>").a(A.ny(a,new A.b6(b.h("@<0>").F(c).h("b6<1,2>"))))},
ac(a,b){return new A.b6(a.h("@<0>").F(b).h("b6<1,2>"))},
jR(a){return new A.aN(a.h("aN<0>"))},
a4(a){return new A.aN(a.h("aN<0>"))},
eg(a,b){return b.h("jQ<0>").a(A.nz(a,new A.aN(b.h("aN<0>"))))},
iU(){var s=Object.create(null)
s["<non-identifier-key>"]=s
delete s["<non-identifier-key>"]
return s},
k6(a,b,c){var s=new A.aO(a,b,c.h("aO<0>"))
s.c=a.e
return s},
lH(a,b){var s=J.bU(a.a)
if(new A.bf(s,a.b,a.$ti.h("bf<1>")).l())return s.gp()
return null},
lN(a,b){var s,r,q=A.jR(b)
for(s=a.length,r=0;r<a.length;a.length===s||(0,A.o)(a),++r)q.i(0,b.a(a[r]))
return q},
iP(a,b){var s=A.jR(b)
s.C(0,a)
return s},
jT(a){var s,r
if(A.jl(a))return"{...}"
s=new A.dx("")
try{r={}
B.b.i($.ai,a)
s.a+="{"
r.a=!0
a.c7(0,new A.ek(r,s))
s.a+="}"}finally{if(0>=$.ai.length)return A.a($.ai,-1)
$.ai.pop()}r=s.a
return r.charCodeAt(0)==0?r:r},
aN:function aN(a){var _=this
_.a=0
_.f=_.e=_.d=_.c=_.b=null
_.r=0
_.$ti=a},
dK:function dK(a){this.a=a
this.c=this.b=null},
aO:function aO(a,b,c){var _=this
_.a=a
_.b=b
_.d=_.c=null
_.$ti=c},
v:function v(){},
ca:function ca(){},
ek:function ek(a,b){this.a=a
this.b=b},
aW:function aW(){},
cz:function cz(){},
m6(a,b,c,d,a0,a1){var s,r,q,p,o,n,m,l,k,j,i="Invalid encoding before padding",h="Invalid character",g=B.a.B(a1,2),f=a1&3,e=$.lk()
for(s=a.length,r=e.length,q=d.$flags|0,p=b,o=0;p<c;++p){if(!(p<s))return A.a(a,p)
n=a.charCodeAt(p)
o|=n
m=n&127
if(!(m<r))return A.a(e,m)
l=e[m]
if(l>=0){g=(g<<6|l)&16777215
f=f+1&3
if(f===0){k=a0+1
q&2&&A.r(d)
m=d.length
if(!(a0<m))return A.a(d,a0)
d[a0]=g>>>16&255
a0=k+1
if(!(k<m))return A.a(d,k)
d[k]=g>>>8&255
k=a0+1
if(!(a0<m))return A.a(d,a0)
d[a0]=g&255
a0=k
g=0}continue}else if(l===-1&&f>1){if(o>127)break
if(f===3){if((g&3)!==0)throw A.f(A.aE(i,a,p))
k=a0+1
q&2&&A.r(d)
s=d.length
if(!(a0<s))return A.a(d,a0)
d[a0]=g>>>10
if(!(k<s))return A.a(d,k)
d[k]=g>>>2}else{if((g&15)!==0)throw A.f(A.aE(i,a,p))
q&2&&A.r(d)
if(!(a0<d.length))return A.a(d,a0)
d[a0]=g>>>4}j=(3-f)*3
if(n===37)j+=2
return A.k4(a,p+1,c,-j-1)}throw A.f(A.aE(h,a,p))}if(o>=0&&o<=127)return(g<<2|f)>>>0
for(p=b;p<c;++p){if(!(p<s))return A.a(a,p)
if(a.charCodeAt(p)>127)break}throw A.f(A.aE(h,a,p))},
m4(a,b,c,d){var s=A.m5(a,b,c),r=(d&3)+(s-b),q=B.a.B(r,2)*3,p=r&3
if(p!==0&&s<c)q+=p-1
if(q>0)return new Uint8Array(q)
return $.lj()},
m5(a,b,c){var s,r=a.length,q=c,p=q,o=0
for(;;){if(!(p>b&&o<2))break
A:{--p
if(!(p>=0&&p<r))return A.a(a,p)
s=a.charCodeAt(p)
if(s===61){++o
q=p
break A}if((s|32)===100){if(p===b)break;--p
if(!(p>=0&&p<r))return A.a(a,p)
s=a.charCodeAt(p)}if(s===51){if(p===b)break;--p
if(!(p>=0&&p<r))return A.a(a,p)
s=a.charCodeAt(p)}if(s===37){++o
q=p
break A}break}}return q},
k4(a,b,c,d){var s,r,q
if(b===c)return d
s=-d-1
for(r=a.length;s>0;){if(!(b<r))return A.a(a,b)
q=a.charCodeAt(b)
if(s===3){if(q===61){s-=3;++b
break}if(q===37){--s;++b
if(b===c)break
if(!(b<r))return A.a(a,b)
q=a.charCodeAt(b)}else break}if((s>3?s-3:s)===2){if(q!==51)break;++b;--s
if(b===c)break
if(!(b<r))return A.a(a,b)
q=a.charCodeAt(b)}if((q|32)!==100)break;++b;--s
if(b===c)break}if(b!==c)throw A.f(A.aE("Invalid padding character",a,b))
return-s-1},
ea:function ea(){},
eP:function eP(){this.a=0},
d6:function d6(){},
eK:function eK(){},
f9:function f9(a){this.b=0
this.c=a},
lA(a,b){a=A.U(a,new Error())
if(a==null)a=A.bF(a)
a.stack=b.j(0)
throw a},
aU(a,b,c,d){var s,r=J.lK(a,d)
if(a!==0&&b!=null)for(s=0;s<a;++s)r[s]=b
return r},
jS(a,b,c){var s,r,q=A.d([],c.h("u<0>"))
for(s=a.length,r=0;r<a.length;a.length===s||(0,A.o)(a),++r)B.b.i(q,c.a(a[r]))
q.$flags=1
return q},
J(a,b){var s,r
if(Array.isArray(a))return A.d(a.slice(0),b.h("u<0>"))
s=A.d([],b.h("u<0>"))
for(r=J.bU(a);r.l();)B.b.i(s,r.gp())
return s},
lO(a,b,c){var s,r=J.lL(a,c)
for(s=0;s<a;++s)B.b.q(r,s,b.$1(s))
return r},
eh(a,b){var s=A.jS(a,!1,b)
s.$flags=3
return s},
lT(a){return new A.c5(a,A.jN(a,!1,!0,!1,!1,""))},
iS(a,b,c){var s=J.bU(b)
if(!s.l())return a
if(c.length===0){do a+=A.k(s.gp())
while(s.l())}else{a+=A.k(s.gp())
while(s.l())a=a+c+A.k(s.gp())}return a},
lW(){return A.bR(new Error())},
eb(a){if(typeof a=="number"||A.j4(a)||a==null)return J.d_(a)
if(typeof a=="string")return JSON.stringify(a)
return A.jX(a)},
lB(a,b){A.fP(a,"error",t.K)
A.fP(b,"stackTrace",t.l)
A.lA(a,b)},
d1(a){return new A.d0(a)},
C(a,b){return new A.ap(!1,null,b,a)},
jG(a,b,c){return new A.ap(!0,a,b,c)},
jZ(a,b){return new A.cg(null,null,!0,a,b,"Value not in range")},
ar(a,b,c,d,e){return new A.cg(b,c,!0,a,d,"Invalid value")},
ch(a,b,c){if(0>a||a>c)throw A.f(A.ar(a,0,c,"start",null))
if(b!=null){if(a>b||b>c)throw A.f(A.ar(b,a,c,"end",null))
return b}return c},
es(a,b){if(a<0)throw A.f(A.ar(a,0,null,b,null))
return a},
ec(a,b,c,d,e){return new A.d8(b,!0,a,e,"Index out of range")},
be(a){return new A.cn(a)},
k2(a){return new A.dB(a)},
eG(a){return new A.cl(a)},
Z(a){return new A.d5(a)},
aE(a,b,c){return new A.c2(a,b,c)},
lI(a,b){if(a<=0)return new A.c_(b.h("c_<0>"))
if(!b.h("0(b)").b(A.kK()))throw A.f(A.C("Generator must be supplied or element type must allow integers","generator"))
return new A.cq(a,A.kK(),b.h("cq<0>"))},
lJ(a,b,c){var s,r
if(A.jl(a)){if(b==="("&&c===")")return"(...)"
return b+"..."+c}s=A.d([],t.s)
B.b.i($.ai,a)
try{A.mV(a,s)}finally{if(0>=$.ai.length)return A.a($.ai,-1)
$.ai.pop()}r=A.iS(b,t.r.a(s),", ")+c
return r.charCodeAt(0)==0?r:r},
iK(a,b,c){var s,r
if(A.jl(a))return b+"..."+c
s=new A.dx(b)
B.b.i($.ai,a)
try{r=s
r.a=A.iS(r.a,a,", ")}finally{if(0>=$.ai.length)return A.a($.ai,-1)
$.ai.pop()}s.a+=c
r=s.a
return r.charCodeAt(0)==0?r:r},
m7(a){return A.j(a)},
mV(a,b){var s,r,q,p,o,n,m,l=a.gt(a),k=0,j=0
for(;;){if(!(k<80||j<3))break
if(!l.l())return
s=A.k(l.gp())
B.b.i(b,s)
k+=s.length+2;++j}if(!l.l()){if(j<=5)return
if(0>=b.length)return A.a(b,-1)
r=b.pop()
if(0>=b.length)return A.a(b,-1)
q=b.pop()}else{p=l.gp();++j
if(!l.l()){if(j<=4){B.b.i(b,A.k(p))
return}r=A.k(p)
if(0>=b.length)return A.a(b,-1)
q=b.pop()
k+=r.length+2}else{o=l.gp();++j
for(;l.l();p=o,o=n){n=l.gp();++j
if(j>100){for(;;){if(!(k>75&&j>3))break
if(0>=b.length)return A.a(b,-1)
k-=b.pop().length+2;--j}B.b.i(b,"...")
return}}q=A.k(p)
r=A.k(o)
k+=r.length+q.length+4}}if(j>b.length+2){k+=5
m="..."}else m=null
for(;;){if(!(k>80&&b.length>3))break
if(0>=b.length)return A.a(b,-1)
k-=b.pop().length+2
if(m==null){k+=5
m="..."}}if(m!=null)B.b.i(b,m)
B.b.i(b,q)
B.b.i(b,r)},
iQ(a,b,c,d){var s
if(B.l===c){s=B.a.gD(a)
b=J.aw(b)
return A.eH(A.aI(A.aI($.e9(),s),b))}if(B.l===d){s=B.a.gD(a)
b=J.aw(b)
c=J.aw(c)
return A.eH(A.aI(A.aI(A.aI($.e9(),s),b),c))}s=B.a.gD(a)
b=J.aw(b)
c=J.aw(c)
d=J.aw(d)
d=A.eH(A.aI(A.aI(A.aI(A.aI($.e9(),s),b),c),d))
return d},
lQ(a){var s,r,q=$.e9()
for(s=a.length,r=0;r<a.length;a.length===s||(0,A.o)(a),++r)q=A.aI(q,J.aw(a[r]))
return A.eH(q)},
eQ:function eQ(){},
D:function D(){},
d0:function d0(a){this.a=a},
aJ:function aJ(){},
ap:function ap(a,b,c,d){var _=this
_.a=a
_.b=b
_.c=c
_.d=d},
cg:function cg(a,b,c,d,e,f){var _=this
_.e=a
_.f=b
_.a=c
_.b=d
_.c=e
_.d=f},
d8:function d8(a,b,c,d,e){var _=this
_.f=a
_.a=b
_.b=c
_.c=d
_.d=e},
cn:function cn(a){this.a=a},
dB:function dB(a){this.a=a},
cl:function cl(a){this.a=a},
d5:function d5(a){this.a=a},
dq:function dq(){},
ck:function ck(){},
eR:function eR(a){this.a=a},
c2:function c2(a,b,c){this.a=a
this.b=b
this.c=c},
h:function h(){},
cq:function cq(a,b,c){this.a=a
this.b=b
this.$ti=c},
W:function W(a,b,c){this.a=a
this.b=b
this.$ti=c},
Y:function Y(){},
x:function x(){},
dQ:function dQ(){},
dx:function dx(a){this.a=a},
iL(a,b){var s,r,q,p,o
if(b.length===0)return!1
s=b.split(".")
r=v.G
for(q=s.length,p=0;p<q;++p,r=o){o=r[s[p]]
A.q(o)
if(o==null)return!1}return a instanceof t.g.a(r)},
en:function en(a){this.a=a},
I(a){var s
if(typeof a=="function")throw A.f(A.C("Attempting to rewrap a JS function.",null))
s=function(b,c){return function(d){return b(c,d,arguments.length)}}(A.aZ,a)
s[$.a8()]=a
return s},
aZ(a,b,c){t.Z.a(a)
if(A.j(c)>=1)return a.$1(b)
return a.$0()},
nF(a,b,c){return c.a(a[b])},
kB(a,b,c,d){return a[b]=c},
cK(a,b,c,d){return d.a(a[b].apply(a,c))},
mu(a,b,c,d){return d.a(a[b](c))},
jt(a,b){var s=new A.T($.K,b.h("T<0>")),r=new A.cp(s,b.h("cp<0>"))
a.then(A.bO(new A.hv(r,b),1),A.bO(new A.hw(r),1))
return s},
hv:function hv(a,b){this.a=a
this.b=b},
hw:function hw(a){this.a=a},
cO(a,b){var s,r,q,p,o=a.a1(b),n=A.d([],t.t)
for(s=a.a.a,r=s.length,q=0;q<8;++q){p=16+(o+q)
if(!(p>=0&&p<r))return A.a(s,p)
n.push(s[p])}return n},
jC(a,b,c){var s,r,q=c.length
if(q!==8)throw A.f(A.C("Expected 8 colors, got "+q,null))
if(B.b.K(c,new A.iE()))throw A.f(A.C("Colors must be 0-63",null))
s=new Uint8Array(A.z(a.a.a))
r=16+a.a1(b)
B.h.R(s,r,r+8,c)
return s},
iE:function iE(){},
j7(a,b,c,d){var s,r,q,p,o,n,m,l,k,j,i,h,g,f=A.jO(b)
for(s=a.a,r=s.length,q=f,p=null;p==null;){o=q+1
n=16+q
if(!(n>=0&&n<r))return A.a(s,n)
m=s[n]
if(m===246){n=16+o
if(!(n>=0&&n<r))return A.a(s,n)
l=s[n]
if(l!==255)c=l
q=o+4}else if(m===252)q=o+1
else if(m===248)q=o+2
else{if(m>=128&&m<240){n=16+o
if(!(n>=0&&n<r))return A.a(s,n)
p=65536+((m<<8|s[n])&32767)}else throw A.f(A.aE("Unknown sprite group command $"+B.a.k(m,16),null,null))
q=o}}k=A.a4(t.R)
for(j=p;;){i=j+1
n=16+j
if(n>>>0!==n||n>=r)return A.a(s,n)
h=s[n]
if(h===255){j=i
break}if(h===252)j=i+1
else if(h<128){j=i+1
n=16+i
if(n>>>0!==n||n>=r)return A.a(s,n)
g=s[n]&127
k.i(0,g<64?new A.i(c,g):new A.i(d,g&63))}else j=i}return new A.bh(f,c,p,j-p,k)},
fD(a,b,c){var s=16+a.b+b*1024+c*16
return new Uint8Array(A.z(B.h.X(a.a,s,s+16)))},
kC(a,b){var s,r,q,p,o,n,m=a.G(b),l=new A.bB($.cY())
for(s=m.d,r=a.a,q=m.y,p=0;p<64;++p){o=s[p]
n=o>>>6
if(!(n<4))return A.a(q,n)
l.i(0,A.fD(r,q[n],o&63))}l.i(0,m.e)
return l.cl()},
kz(a,b){var s,r,q=a.length,p=b.length
if(q!==p)return!1
for(s=0;s<q;++s){r=a[s]
if(!(s<p))return A.a(b,s)
if(r!==b[s])return!1}return!0},
np(a,b){var s,r,q,p,o,n,m,l,k,j,i,h,g,f,e=a.a,d=A.d([],t.t)
for(s=e.a,r=s.length,q=0;q<8;++q){p=16+(a.a1(b)+q)
if(!(p>=0&&p<r))return A.a(s,p)
d.push(s[p])}o=new Uint8Array(A.z(d))
n=A.j7(e,a.al(b),112,115)
m=A.ac(t.S,t.p)
l=n.c
for(d=n.b,k=l;;){j=k+1
p=16+k
if(!(p<r))return A.a(s,p)
i=s[p]
if(i===255)break
if(i===252)k=j+1
else if(i<128){k=j+1
p=16+j
if(!(p<r))return A.a(s,p)
h=s[p]&127
m.q(0,h,h<64?A.fD(e,d,h):A.fD(e,115,h&63))}else k=j}d=a.b?B.e:B.d
if(!(b<d.length))return A.a(d,b)
d=d[b]
p=A.kC(a,b)
g=n.a
f=16+(g+8)
if(!(f>=0&&f<r))return A.a(s,f)
f=s[f]
g=16+(g+9)
if(!(g>=0&&g<r))return A.a(s,g)
r=16+l
return new A.d7(d,o,p,f,s[g],new Uint8Array(A.z(B.h.X(s,r,r+n.d))),m)},
l_(a6,a7,a8,a9,b0,b1){var s,r,q,p,o,n,m,l,k,j,i,h,g,f,e,d,c,b,a,a0,a1=t.s,a2=A.d([],a1),a3=A.d([],a1),a4=A.d([],a1),a5=A.bb(new Uint8Array(A.z(a6.a.a)))
a1=A.aq(a5)
s=new A.a1(a5,a1)
r=a1?B.e:B.d
if(!(a7<r.length))return A.a(r,a7)
q=r[a7]
p=new A.hu(a5)
if(a9){o=s.a1(a7)
for(r=a8.c,n=r.length,m=0;m<8;++m){if(!(m<n))return A.a(r,m)
p.$2(o+m,r[m])}B.b.i(a3,"Colors: copied (shell, facemask, logo).")}if(b1){r=a5.a
n=16+s.aq(a7)
l=r.length
if(!(n>=0&&n<l))return A.a(r,n)
k=r[n]
n=[k,64,65,66,67]
i=a8.d
h=t.t
g=0
for(;;){if(!(g<5)){j=null
break}A:{f=n[g]
e=A.d([],h)
d=0
for(;;){if(!(d<(a1?B.e:B.d).length))break
c=a1&&d>=28?d+2:d
if(a1)b=(d>=28?d+2:d)>=30
else b=!1
a=16+(b?259674+c-30:147017+c)
if(!(a<l))return A.a(r,a)
if(r[a]===f)e.push(d);++d}if(e.length===0)break A
if(A.kz(A.kC(s,B.b.ga0(e)),i)){j=f
break}}++g}if(j==null)B.b.i(a3,"Shell: "+a8.a+"'s shell isn't in this ROM; "+q+" keeps its own.")
else{p.$2(s.aq(a7),j)
B.b.i(a3,j===k?"Shell: same as before ($"+B.a.k(j,16).toUpperCase()+").":"Shell: switched to $"+B.a.k(j,16).toUpperCase()+".")}}if(b0){a0=A.n1(s,a7,a8,a3,a4)
if(a0!=null)B.b.i(a2,a0)}return a2.length===0?new A.ax(a2,a3,a4,a5.a):new A.ax(a2,a3,a4,null)},
n1(d4,d5,d6,d7,d8){var s,r,q,p,o,n,m,l,k,j,i,h,g,f,e,d,c,b,a,a0,a1,a2,a3,a4,a5,a6,a7,a8,a9,b0,b1,b2,b3,b4,b5,b6,b7,b8,b9,c0,c1,c2,c3,c4,c5,c6,c7=null,c8=d4.a,c9=new A.fz(c8),d0=t.S,d1=A.ac(d0,t.L),d2=d4.b,d3=0
for(;;){if(!(d3<(d2?B.e:B.d).length))break
J.cZ(d1.a2(d4.al(d3),new A.ft()),d3);++d3}s=t.x
r=A.ac(d0,s)
for(q=new A.b8(d1,d1.r,d1.e,d1.$ti.h("b8<1>"));q.l();){p=q.d
r.q(0,p,A.j7(c8,p,112,115))}o=A.j7(c8,c8.aa(65694),112,115)
n=d4.al(d5)
m=r.v(0,n)
q=m.a
p=c8.a
l=16+q
k=p.length
if(!(l>=0&&l<k))return A.a(p,l)
j=!1
if(p[l]===246){l=16+(q+5)
if(!(l>=0&&l<k))return A.a(p,l)
if(p[l]===252){l=16+(q+7)
if(!(l>=0&&l<k))return A.a(p,l)
if(p[l]===248){l=16+(q+10)
if(!(l>=0&&l<k))return A.a(p,l)
l=(p[l]&224)===128}else l=j}else l=j}else l=j
if(!l){d0=d2?B.e:B.d
if(!(d5<d0.length))return A.a(d0,d5)
return d0[d5]+"'s logo header has an unexpected layout; not touching it."}for(l=d1.v(0,n),j=l.length,i=0;i<l.length;l.length===j||(0,A.o)(l),++i){d3=l[i]
if(d3!==d5){h=d2?B.e:B.d
if(!(d3>=0&&d3<h.length))return A.a(h,d3)
B.b.i(d8,h[d3])}}l=A.iP(o.e,t.R)
for(j=r.$ti,h=j.h("N<1,2>"),g=new A.N(r,h).gt(0);g.l();){f=g.d
if(f.a!==n)l.C(0,f.b.e)}g=A.a4(d0)
s=A.J(new A.c8(r,j.h("c8<2>")),s)
s.push(o)
e=s.length
i=0
for(;i<s.length;s.length===e||(0,A.o)(s),++i){d=s[i].e
c=A.A(d)
g.C(0,new A.bZ(d,c.h("b(1)").a(new A.fu()),c.h("bZ<1,b>")))}for(s=new A.aG(r,r.r,r.e,j.h("aG<2>"));s.l();)g.i(0,s.d.b)
s=t.N
b=A.ac(s,t.p)
for(j=d6.w,e=new A.aG(j,j.r,j.e,A.A(j).h("aG<2>"));e.l();){d=e.d
b.q(0,B.h.H(d,","),d)}e=m.b
d=t.t
a=A.d([e],d)
if(d2)for(d2=B.a.u(c8.c,1024),c=16+c8.b,a0=128;a0<d2;++a0){if(g.J(0,a0))continue
a1=c+a0*1024
if(B.h.M(new Uint8Array(p.subarray(a1,A.km(a1,a1+1024,k))),new A.fv())){B.b.i(a,a0)
break}}d2=a.length
k=b.$ti
g=k.h("c7<1>")
c=g.h("m(h.E)")
a4=g.h("aL<h.E>")
a5=a4.h("h.E")
k=k.h("b7<1,2>")
a6=m.e
a7=A.A(l)
a8=a7.h("aO<1>")
a7=a7.c
a9=0
b0=0
i=0
for(;;){if(!(i<a.length)){a2=c7
a3=a2
break}A:{a0=a[i]
b1=A.a4(d0)
for(b2=new A.aO(l,l.r,a8),b2.c=l.e;b2.l();){b3=b2.d
if(b3==null)b3=a7.a(b3)
if(b3.a===a0)b1.i(0,b3.b)}b2=a0===e
b4=A.d([],d)
for(b5=0;b5<64;++b5){if(!b1.J(0,b5))b6=!b2||a6.J(0,new A.i(a0,b5))
else b6=!1
if(b6)b4.push(b5)}b7=A.ac(s,d0)
b8=A.J(b4,d0)
for(b4=new A.b7(b,b.r,b.e,k);b4.l();){f=b4.d
for(b6=f.b,b5=0;b5<64;++b5)if((b1.J(0,b5)||B.b.J(b8,b5))&&A.kz(A.fD(c8,a0,b5),b6)){b7.q(0,f.a,b5)
B.b.aV(b8,b5)
break}}b9=A.J(new A.aL(new A.c7(b,g),c.a(new A.fw(b7)),a4),a5)
if(b2){a9=b9.length
b0=b8.length}b1=b9.length
if(b1>b8.length)break A
for(i=0;i<b9.length;b9.length===b1||(0,A.o)(b9),++i)b7.q(0,b9[i],B.b.ao(b8,0))
a2=a0
a3=b7
break}a.length===d2||(0,A.o)(a);++i}if(a3==null||a2==null){d0=A.d([],t.s)
for(d2=new A.N(r,h).gt(0);d2.l();){f=d2.d
r=f.a
if(r!==n&&f.b.b===e){r=d1.v(0,r)
r.toString
B.b.C(d0,J.jF(r,new A.fx(d4),s))}}d2=a9===1?"":"s"
return"Logo needs "+a9+" new tile"+d2+" but only "+b0+" are free in CHR bank $"+B.a.k(e,16).toUpperCase()+" (shared with "+B.b.H(d0,", ")+")."}c0=new Uint8Array(A.z(d6.r))
d0=c0.length
c1=0
for(;;){if(!(c1<d0))return A.a(c0,c1)
d2=c0[c1]
if(!(d2!==255))break;++c1
if(d2===252)++c1
else if(d2<128){if(!(c1<d0))return A.a(c0,c1)
d2=j.v(0,c0[c1]&127)
d2.toString
d2=a3.v(0,B.h.H(d2,","))
d2.toString
c0.$flags&2&&A.r(c0)
c0[c1]=d2;++c1}}c2=new A.N(r,h).K(0,new A.fy(n,m))
d2=!c2
if(d2&&d0<=m.d)c3=m.c
else{c4=A.mz(c8,d0,c2?c7:new A.i(m.c,m.d))
if(c4==null)return"No free space left for the logo layout ("+d0+" bytes)."
c3=c4}if(d2)for(d2=m.d,s=m.c,b5=0;b5<d2;++b5)c9.$2(s+b5,255)
for(b5=0;b5<d0;++b5)c9.$2(c3+b5,c0[b5])
c5=16+c8.b+a2*1024
for(d2=new A.N(a3,a3.$ti.h("N<1,2>")).gt(0);d2.l();){f=d2.d
s=c5+f.b*16
r=b.v(0,f.a)
r.toString
B.h.R(p,s,s+16,r)}c9.$2(q+1,a2)
c9.$2(q+2,255)
c9.$2(q+3,255)
c9.$2(q+4,255)
c9.$2(q+8,d6.e)
c9.$2(q+9,d6.f)
c6=32768+c3-65536
c9.$2(q+10,B.a.B(c6,8))
c9.$2(q+11,c6&255)
d2=b.a
s=B.a.k(a2,16)
r=a2===e?"":" (empty expansion bank)"
q=c3===m.c?"in place":"moved to free space"
B.b.i(d7,"Logo: "+d2+" tiles in CHR bank $"+s.toUpperCase()+r+", "+d0+"-byte layout "+q+".")
return c7},
mz(a,b,c){var s,r,q=new A.fh(a,c)
for(s=-1,r=65536;r<73728;++r)if(q.$1(r)){if(s<0)s=r+1
if(r-s+1>=b)return s}else s=-1
return null},
d7:function d7(a,b,c,d,e,f,g){var _=this
_.a=a
_.c=b
_.d=c
_.e=d
_.f=e
_.r=f
_.w=g},
ax:function ax(a,b,c,d){var _=this
_.a=a
_.b=b
_.c=c
_.e=d},
bh:function bh(a,b,c,d,e){var _=this
_.a=a
_.b=b
_.c=c
_.d=d
_.e=e},
hu:function hu(a){this.a=a},
fz:function fz(a){this.a=a},
ft:function ft(){},
fu:function fu(){},
fv:function fv(){},
fw:function fw(a){this.a=a},
fx:function fx(a){this.a=a},
fy:function fy(a,b){this.a=a
this.b=b},
fh:function fh(a,b){this.a=a
this.b=b},
cT(a1,a2,a3){var s,r,q,p,o,n,m,l,k,j,i,h,g,f,e,d,c,b=a2.x,a=new Uint8Array(5184),a0=b.length
if(0>=a0)return A.a(b,0)
B.h.a7(a,0,5184,b[0])
s=new A.hI(32,32,72,72,a)
r=a2.d
q=a2.y
p=a2.e
o=0
for(;;){if(!(o<8))break
for(n=o*8,m=33+n,l=(o>>>1)*4,k=0;k<8;++k){j=n+k
if(!(j<64))return A.a(r,j)
i=r[j]
j=i>>>6
if(!(j<4))return A.a(q,j)
h=a1.a3(q[j],i)
j=l+(k>>>1)
if(!(j<16))return A.a(p,j)
for(g=32+k*8,j=p[j]*4,f=0;f<64;++f){if(!(f<h.length))return A.a(h,f)
e=h[f]
if(e!==0){d=B.a.A(f,8)
c=B.a.u(f,8)
e=j+e
if(!(e>=0&&e<a0))return A.a(b,e)
s.$3(g+d,m+c,b[e])}}}++o}for(r=a2.f,q=A.Q(r).h("aH<1>"),r=new A.aH(r,q),r=new A.ad(r,r.gm(0),q.h("ad<O.E>")),q=q.h("O.E");r.l();){p=r.d
if(p==null)p=q.a(p)
n=p.e
if(!(n!==3||a3))continue
h=a1.a3(p.b,p.a)
for(m=p.d,n=16+n*4,l=p.f,p=p.c,f=0;f<64;++f){if(!(f<h.length))return A.a(h,f)
j=h[f]
if(j!==0){g=l?7-B.a.A(f,8):B.a.A(f,8)
e=B.a.u(f,8)
j=n+j
if(!(j>=0&&j<a0))return A.a(b,j)
s.$3(p+g,m+e,b[j])}}}return a},
nL(a){var s,r,q,p,o=a.length,n=o*4,m=new Uint8Array(n)
for(s=0;s<o;++s){r=a[s]
if(!(r<64))return A.a(B.f,r)
q=B.f[r]
r=s*4
if(!(r<n))return A.a(m,r)
m[r]=q>>>16
p=r+1
if(!(p<n))return A.a(m,p)
m[p]=q>>>8&255
p=r+2
if(!(p<n))return A.a(m,p)
m[p]=q&255
r+=3
if(!(r<n))return A.a(m,r)
m[r]=255}return m},
hI:function hI(a,b,c,d,e){var _=this
_.a=a
_.b=b
_.c=c
_.d=d
_.e=e},
aq(a){var s,r,q=!1
if(a.c===262144){s=a.a
r=s.length
if(141530>=r)return A.a(s,141530)
if(s[141530]===32){if(141531>=r)return A.a(s,141531)
if(s[141531]===74){if(141532>=r)return A.a(s,141532)
q=s[141532]===246}}}return q},
jO(a){return a>=40960?65536+a-40960:188416+a-32768},
aa:function aa(a,b,c,d,e,f){var _=this
_.a=a
_.b=b
_.c=c
_.d=d
_.e=e
_.f=f},
ee:function ee(a,b,c,d,e){var _=this
_.d=a
_.e=b
_.f=c
_.x=d
_.y=e},
a1:function a1(a,b){this.a=a
this.b=b},
jo(a,b){var s,r,q,p,o,n,m,l,k,j,i,h=new Uint8Array(5184)
for(s=a.G(b).f,r=A.Q(s).h("aH<1>"),s=new A.aH(s,r),s=new A.ad(s,s.gm(0),r.h("ad<O.E>")),r=r.h("O.E");s.l();){q=s.d
if(q==null)q=r.a(q)
if(q.e!==3)continue
p=A.kD(a,q)
for(o=p.length,n=q.c,q=q.d,m=0;m<64;++m){l=n+B.a.A(m,8)-32
k=q+B.a.u(m,8)-32
if(!(m<o))return A.a(p,m)
j=p[m]
if(j!==0&&l>=0&&l<72&&k>=0&&k<72){i=k*72+l
if(!(i>=0&&i<5184))return A.a(h,i)
h[i]=j}}}return h},
kD(a,b){var s,r,q,p=a.a.a3(b.b,b.a),o=A.d([],t.t)
for(s=b.f,r=0;r<64;++r){if(s){q=B.a.A(r,8)
q=r-q+7-q}else q=r
if(!(q>=0&&q<p.length))return A.a(p,q)
o.push(p[q])}return new Uint8Array(A.z(o))},
jf(a,b,c,d){var s,r,q,p,o,n,m,l,k,j,i
if(c.length!==5184)throw A.f(A.C("Expected 5184 pixels",null))
s=A.mY(a,b)
r=new A.fN(s,a,b)
q=new A.fO(s)
if(d!=null){p=A.d([],t.n)
for(o=d.length,n=0;n<d.length;d.length===o||(0,A.o)(d),++n){m=d[n]
p.push(new A.i(m.a+32,m.b+32))}p=A.kw(a,b,c,p)
p.toString
l=r.$1(p)
return q.$3(p,l,l.e==null&&A.jf(a,b,c,null).b.e!=null)}k=A.mW(a,b,c)
for(p=k.length,j=null,n=0;n<k.length;k.length===p||(0,A.o)(k),++n){i=k[n]
l=r.$1(i)
if(l.e!=null)return q.$3(i,l,!1)
if(j==null)j=new A.i(i,l)}return q.$3(j.a,j.b,!1)},
kT(a,b,c,a0,a1){var s,r,q,p,o,n,m,l,k,j,i,h,g,f,e,d
if(!(c>=0&&c<b.length))return A.a(b,c)
s=b[c]
r=s.a
q=s.b
p=r<0?r:0
o=r>64?r:64
n=B.a.V(r+a0,p,o)
p=q<0?q:0
o=q>64?q:64
m=B.a.V(q+a1,p,o)
l=new A.hp(b,c)
k=new A.hr()
j=new Uint8Array(A.z(a))
i=new Uint8Array(64)
for(p=a.length,h=0;h<64;++h){g=r+B.a.A(h,8)
f=q+B.a.u(h,8)
if(!k.$2(g,f)||l.$2(g,f))continue
o=f*72+g
if(!(o>=0&&o<p))return A.a(a,o)
e=a[o]
if(!(h<64))return A.a(i,h)
i[h]=e
j.$flags&2&&A.r(j)
if(!(o<j.length))return A.a(j,o)
j[o]=0}for(h=0;h<64;++h){g=n+B.a.A(h,8)
f=m+B.a.u(h,8)
if(i[h]!==0&&k.$2(g,f)){p=f*72+g
o=i[h]
j.$flags&2&&A.r(j)
if(!(p>=0&&p<j.length))return A.a(j,p)
j[p]=o}}p=A.d([new A.i(n,m)],t.n)
for(d=0;d<b.length;++d)if(d!==c)p.push(b[d])
return new A.cw(p,j)},
mY(a,b){var s,r,q,p,o=A.d([],t.U)
for(s=a.G(b).f,r=s.length,q=0;q<s.length;s.length===r||(0,A.o)(s),++q){p=s[q]
if(p.e===2)o.push(p)}return o},
mW(a,b,c){var s,r,q,p,o,n=A.d([],t.n)
for(s=A.kv(a,b),r=s.length,q=0;q<s.length;s.length===r||(0,A.o)(s),++q){p=s[q]
n.push(new A.i(p.c,p.d))}o=A.kw(a,b,c,n)
n=A.d([],t.q)
if(o!=null)n.push(o)
n.push(A.kq(c))
return n},
kv(a,b){var s,r,q,p,o=A.d([],t.U)
for(s=a.G(b).f,r=s.length,q=0;q<s.length;s.length===r||(0,A.o)(s),++q){p=s[q]
if(p.e===3)o.push(p)}return o},
fb(a,b,c){var s,r=b-32,q=c-32
if(r>=0&&r<72&&q>=0&&q<72){s=q*72+r
if(!(s>=0&&s<a.length))return A.a(a,s)
s=a[s]}else s=0
return s},
kw(a,b,c,d){var s,r,q,p,o,n,m,l,k,j,i,h=A.kv(a,b),g=A.d([],t.cj)
for(s=d.length,r=0;r<d.length;d.length===s||(0,A.o)(d),++r){q={}
p=d[r]
q.a=q.b=null
q.b=p.a
q.a=p.b
g.push(new A.fl(q,h).$0())}o=new A.fo(d)
s=c.length
n=new Uint8Array(s)
for(m=0;m<s;++m)if(c[m]!==0&&!o.$2(32+B.a.A(m,72),32+B.a.u(m,72))){q=c[m]
if(!(m<s))return A.a(n,m)
n[m]=q}l=B.h.K(n,new A.fm())?A.kq(n):A.d([],t.Y)
k=new A.fq(d,g,A.jo(a,b),c,a)
for(g=[k.$1$keep(!0),k.$1$keep(!1)],s=t.Y,r=0;r<2;++r){j=g[r]
q=A.d([],s)
for(i=0;i<d.length;++i){if(!(i<j.length))return A.a(j,i)
if(B.h.K(j[i],new A.fn())){if(!(i<d.length))return A.a(d,i)
p=d[i]
if(!(i<j.length))return A.a(j,i)
q.push(new A.a5(p.a,p.b,new Uint8Array(A.z(j[i]))))}}B.b.C(q,l)
if(A.n6(A.my(q),c))return q}return null},
kq(a){var s,r,q,p,o,n,m,l,k,j,i,h
for(s=t.t,r=t.Y,q=null,p=0;p<8;++p){o=A.d([],r)
for(n=p-8;n<72;n+=8){m=new A.fi(n,a)
for(l=32+n,k=0;k<72;){if(!m.$1(k)){++k
continue}j=32+k
i=A.d([],s)
for(h=0;h<64;++h)i.push(A.fb(a,l+B.a.A(h,8),j+B.a.u(h,8)))
B.b.i(o,new A.a5(l,j,new Uint8Array(A.z(i))))
k+=8}}if(q==null||o.length<q.length)q=o}q.toString
return q},
my(a){var s,r,q,p,o,n,m,l,k,j,i,h=new Uint8Array(5184)
for(s=A.Q(a).h("aH<1>"),r=new A.aH(a,s),r=new A.ad(r,r.gm(0),s.h("ad<O.E>")),s=s.h("O.E");r.l();){q=r.d
if(q==null)q=s.a(q)
for(p=q.c,o=p.length,n=q.a,q=q.b,m=0;m<64;++m){l=n+B.a.A(m,8)-32
k=q+B.a.u(m,8)-32
if(!(m<o))return A.a(p,m)
j=p[m]
if(j!==0&&l>=0&&l<72&&k>=0&&k<72){i=k*72+l
if(!(i>=0&&i<5184))return A.a(h,i)
h[i]=j}}}return h},
n6(a,b){var s,r,q
for(s=b.length,r=0;r<5184;++r){q=a[r]
if(!(r<s))return A.a(b,r)
if(q!==b[r])return!1}return 5184===s},
kl(a,b){var s,r,q,p,o,n,m=t.S,l=A.aU(264,0,!1,m)
m=A.J(J.jF(a,new A.fe(),m),m)
s=A.Q(b)
B.b.C(m,new A.ae(b,s.h("b(1)").a(new A.ff()),s.h("ae<1,b>")))
s=m.length
r=0
for(;r<m.length;m.length===s||(0,A.o)(m),++r){q=m[r]
for(p=0;p<8;++p){o=q+p
if(!(o>=0&&o<264))return A.a(l,o)
B.b.q(l,o,l[o]+1)}}for(n=0,q=0;q<264;++q){m=l[q]
if(!(n>=0&&n<264))return A.a(l,n)
if(m>l[n])n=q}if(!(n>=0&&n<264))return A.a(l,n)
return new A.i(n,l[n])},
ne(a,b,c){var s,r,q,p,o,n,m,l,k,j,i,h,g,f=t.S,e=A.ac(t.N,f),d=A.ac(f,t.p)
f=A.d([],t.bO)
for(s=c.length,r=0;r<c.length;c.length===s||(0,A.o)(c),++r)f.push(new A.fF(c[r],e,d).$0())
s=t.t
q=A.d([],s)
for(p=f.length,o=null,n=0,m=0,r=0;r<f.length;f.length===p||(0,A.o)(f),++r){l=f[r].a
if(!J.S(o,new A.i(l[0],l[1]))){k=l[1]
if(k!==n){B.b.C(q,A.d([252,(k|48)>>>0],s))
n=k}B.b.i(q,l[0]&31|128)
o=new A.i(l[0],l[1])}j=l[2]
if(j!==m){B.b.C(q,A.d([252,(j|32)>>>0],s))
m=j}B.b.C(q,A.d([(l[4]&15)<<2|1,l[3]],s))}B.b.i(q,255)
i=a.a1(b)
f=a.b?B.e:B.d
if(!(b<f.length))return A.a(f,b)
f=f[b]
s=A.d([],s)
for(p=a.a.a,l=p.length,h=0;h<8;++h){g=16+(i+h)
if(!(g>=0&&g<l))return A.a(p,g)
s.push(p[g])}s=new Uint8Array(A.z(s))
return new A.d7(f,s,new Uint8Array(0),64,64,new Uint8Array(A.z(q)),d)},
mw(a){var s,r,q,p,o,n=new Uint8Array(16)
for(s=a.length,r=0;r<64;++r){q=B.a.a_(128,B.a.A(r,8))
if(!(r<s))return A.a(a,r)
if((a[r]&1)!==0){p=B.a.u(r,8)
if(!(p<16))return A.a(n,p)
o=n[p]
if(!(p<16))return A.a(n,p)
n[p]=o|q}if((a[r]&2)!==0){p=8+B.a.u(r,8)
if(!(p<16))return A.a(n,p)
o=n[p]
if(!(p<16))return A.a(n,p)
n[p]=o|q}}return n},
ej:function ej(a,b,c,d,e){var _=this
_.a=a
_.b=b
_.c=c
_.d=d
_.e=e},
c9:function c9(a,b,c){this.a=a
this.b=b
this.c=c},
fN:function fN(a,b,c){this.a=a
this.b=b
this.c=c},
fO:function fO(a){this.a=a},
hp:function hp(a,b){this.a=a
this.b=b},
hq:function hq(a,b){this.a=a
this.b=b},
hr:function hr(){},
a5:function a5(a,b,c){this.a=a
this.b=b
this.c=c},
fl:function fl(a,b){this.a=a
this.b=b},
fk:function fk(a){this.a=a},
fo:function fo(a){this.a=a},
fp:function fp(a,b){this.a=a
this.b=b},
fm:function fm(){},
fq:function fq(a,b,c,d,e){var _=this
_.a=a
_.b=b
_.c=c
_.d=d
_.e=e},
fs:function fs(a,b,c,d,e,f,g,h){var _=this
_.a=a
_.b=b
_.c=c
_.d=d
_.e=e
_.f=f
_.r=g
_.w=h},
fr:function fr(a,b){this.a=a
this.b=b},
fn:function fn(){},
fi:function fi(a,b){this.a=a
this.b=b},
fe:function fe(){},
ff:function ff(){},
fF:function fF(a,b,c){this.a=a
this.b=b
this.c=c},
fE:function fE(a,b,c){this.a=a
this.b=b
this.c=c},
nS(a,b){var s,r,q,p,o,n=new Uint8Array(24192),m=new A.h1(n)
for(s=a.length,r=0;r<s;++r){q=a[r]
if(q!==0){--q
if(!(q>=0&&q<b.length))return A.a(b,q)
m.$2(r,b[q])}}for(p=74;p<84;++p)for(s=p*72,o=0;o<72;++o){q=B.a.u(o,24)
if(!(q<b.length))return A.a(b,q)
m.$2(s+o,b[q])}return n},
of(a2,a3,a4){var s,r,q,p,o,n,m,l,k,j,i,h,g,f,e,d,c=new A.hD(a2),b=new A.hC(a2),a=A.d([],t.s),a0=A.J(a4,t.S),a1=A.d([],t.t)
for(s=a4.length,r=0;r<a4.length;a4.length===s||(0,A.o)(a4),++r)a1.push(B.f[a4[r]&63])
if(a3===84){s=A.d([],t.a)
for(q=0;q<3;++q)s.push(new A.hB(q,b,c).$0())
p=t.d
o=A.J(new A.bA(s,p),p.h("h.E"))
if(A.lN(o,A.Q(o).c).a<o.length)B.b.i(a,"Two blocks in the color strip are the same color, so the strip was ignored.")
else for(q=0;q<3;++q){if(!(q<s.length))return A.a(s,q)
n=s[q]
if(n!=null){if(!(q<a1.length))return A.a(a1,q)
p=n===a1[q]}else p=!0
if(p)continue
B.b.q(a1,q,n)
m=A.oc(n)
if(!(q<a4.length))return A.a(a4,q)
if(m===a4[q])continue
B.b.q(a0,q,m)
if(!(q<a4.length))return A.a(a4,q)
p=B.c.n(B.a.k(a4[q],16),2,"0")
l=B.c.n(B.a.k(m,16),2,"0")
if(!(m<64))return A.a(B.f,m)
k=B.f[m]===n?"":" (the closest NES color to #"+B.c.n(B.a.k(n,16),6,"0").toUpperCase()+")"
B.b.i(a,"Color "+(q+1)+" changed from "+("$"+p.toUpperCase())+" to "+("$"+l.toUpperCase())+" from the color strip"+k+".")}}for(j=!1,i=0;i<5184;++i)if(!b.$1(i))j=!0
h=j?null:c.$1(0)
if(h!=null)B.b.i(a,u.b)
g=new Uint8Array(5184)
for(i=0;i<5184;++i){if(!b.$1(i)||J.S(c.$1(i),h))continue
for(f=0,e=1073741824,q=0;q<3;++q){s=c.$1(i)
if(!(q<a1.length))return A.a(a1,q)
d=A.j2(s,a1[q])
if(d<e){f=q+1
e=d}}if(!(i<5184))return A.a(g,i)
g[i]=f}return new A.ei(g,a0,a)},
oc(a){var s,r,q,p=A.j2(a,0)
for(s=15,r=0;r<64;++r){if(r===13)continue
q=A.j2(a,B.f[r])
if(q<p){p=q
s=r}}return s},
j2(a,b){var s=B.a.B(a,16)-B.a.B(b,16),r=(B.a.B(a,8)&255)-(B.a.B(b,8)&255),q=(a&255)-(b&255)
return s*s+r*r+q*q},
h1:function h1(a){this.a=a},
ei:function ei(a,b,c){this.a=a
this.b=b
this.c=c},
hD:function hD(a){this.a=a},
hC:function hC(a){this.a=a},
hB:function hB(a,b,c){this.a=a
this.b=b
this.c=c},
hy:function hy(){},
hz:function hz(){},
hA:function hA(){},
bb(a){var s,r,q=a.length,p=!0
if(q>=16){if(0>=q)return A.a(a,0)
if(a[0]===78){if(1>=q)return A.a(a,1)
if(a[1]===69){if(2>=q)return A.a(a,2)
if(a[2]===83){if(3>=q)return A.a(a,3)
p=a[3]!==26}}}}if(p)throw A.f(B.I)
if(4>=q)return A.a(a,4)
s=a[4]*16384
if(5>=q)return A.a(a,5)
r=a[5]*8192
if(q<16+s+r)throw A.f(B.J)
return new A.em(a,s,r)},
em:function em(a,b,c){this.a=a
this.b=b
this.c=c},
dv:function dv(a,b,c,d){var _=this
_.a=a
_.b=b
_.c=c
_.d=d},
eu:function eu(a,b,c,d,e){var _=this
_.a=a
_.b=b
_.c=c
_.e=d
_.f=e},
bc:function bc(a,b){this.a=a
this.b=b},
eE:function eE(){},
k0(a,b){var s=A.d([],t.a),r=A.d([],t.A),q=t.S,p=t.p
p=new A.ev(a,b,new A.bc(a,new A.a1(a,A.aq(a))),new A.bc(a,new A.a1(a,A.aq(a))).G(b),s,r,A.ac(q,p),A.ac(q,p))
p.bG(a,b)
return p},
lV(a,b){var s,r,q,p
for(s=a.length,r=b.length,q=0;q<s;++q){p=a[q]
if(!(q<r))return A.a(b,q)
if(p!==b[q])return!1}return!0},
eD:function eD(a,b,c,d,e){var _=this
_.a=a
_.b=b
_.c=c
_.d=d
_.e=e},
er:function er(a){this.b=this.a=0
this.c=a},
aM:function aM(a,b,c,d,e,f){var _=this
_.a=a
_.b=b
_.c=c
_.d=d
_.e=e
_.f=f},
ev:function ev(a,b,c,d,e,f,g,h){var _=this
_.a=a
_.b=b
_.c=c
_.d=d
_.e=e
_.f=f
_.r=g
_.w=h
_.x=$
_.y=null},
ez:function ez(){},
eA:function eA(a,b){this.a=a
this.b=b},
eB:function eB(a,b){this.a=a
this.b=b},
eC:function eC(a,b){this.a=a
this.b=b},
ey:function ey(){},
ew:function ew(){},
ex:function ex(){},
op(a){var s,r,q=new Uint8Array(3456),p=new A.iz(q),o=a.aH(),n=a.aT()
for(s=o.length,r=0;r<s;++r)p.$3(4+B.a.A(r,16),B.a.u(r,16),o[r])
for(s=n.length,r=0;r<s;++r)p.$3(B.a.A(r,24),18+B.a.u(r,24),n[r])
return q},
og(a6,a7){var s,r,q,p,o,n,m,l,k,j,i,h,g,f,e,d,c,b,a,a0,a1,a2,a3,a4=new A.hH(a6),a5=A.d([],t.s)
for(s=a6.length,r=!1,q=3;q<s;q+=4)if(a6[q]<128)r=!0
p=r?null:a4.$2(0,0)
if(p!=null)B.b.i(a5,u.b)
o=new A.hF(a7)
n=new A.hE(new A.hG(a6),a4,p)
m=a7.ga5()
l=a7.T().a
k=new Uint8Array(256)
for(s=t.S,j=a7.d.c,i=0;i<16;++i)for(h=i*16,g=(i>>>3)*2,f=0;f<16;++f){e=f+4
if(n.$2(e,i))continue
d=g+(f>>>3)
c=h+f
b=a4.$2(e,i)
a=a7.x
if(a===$){a=A.J(j,s)
a7.x=a}a0=A.jS(a,!1,s)
a0.$flags=3
a1=a0
if(!(d<a1.length))return A.a(a1,d)
a1=a1[d]
if(!(c<256))return A.a(m,c)
B.h.q(k,c,o.$3(b,a1*4,m[c]))}a2=new Uint8Array(432)
for(i=0;i<18;++i)for(s=i*24,j=18+i,f=0;f<24;++f){if(n.$2(f,j))continue
h=s+f
g=a4.$2(f,j)
c=a7.bu(f,i)
if(!(h<432))return A.a(l,h)
B.h.q(a2,h,o.$3(g,16+c*4,l[h]))}for(a3=0,i=0;i<16;++i)for(f=0;f<24;++f)if((f<4||f>=20)&&!n.$2(f,i))++a3
for(i=16;i<18;++i)for(f=0;f<24;++f)if(!n.$2(f,i))++a3
if(a3>0){s=a3===1?"":"s"
B.b.i(a5,""+a3+" pixel"+s+" outside the two drawing areas were ignored.")}return new A.eF(k,a2,a5)},
iz:function iz(a){this.a=a},
eF:function eF(a,b,c){this.a=a
this.b=b
this.c=c},
hH:function hH(a){this.a=a},
hG:function hG(a){this.a=a},
hF:function hF(a){this.a=a},
hE:function hE(a,b,c){this.a=a
this.b=b
this.c=c},
kM(a5,a6,a7,a8,a9,b0,b1){var s,r,q,p,o,n,m,l,k,j,i,h,g,f,e,d,c,b,a,a0,a1=a6.f,a2=t.S,a3=A.a4(a2),a4=new A.fU(a8,a9,a7)
for(s=b1+1,r=a6.c,q=a6.b,p=0;p<4;++p){if(!(p<q.length))return A.a(q,p)
o=q[p]
n=o>>>6
if(!(n<4))return A.a(B.k,n)
m=a5.a3(B.k[n],o)
for(n=b0+(p&1)*8,l=s+(p>>>1)*8,k=0;k<64;++k){if(!(k<m.length))return A.a(m,k)
j=m[k]
if(j===0)continue
i=n+B.a.A(k,8)
h=l+B.a.u(k,8)
j=r[p]*4+j
if(!(j>=0&&j<32))return A.a(a1,j)
a4.$3(i,h,a1[j])
a3.i(0,h*a8+i)}}g=A.a4(a2)
for(a2=a6.e,s=a2.length,f=0;f<a2.length;a2.length===s||(0,A.o)(a2),++f){e=a2[f]
r=e.c
q=r>>>6
if(!(q<4))return A.a(B.j,q)
m=a5.a3(B.j[q],r)
for(r=e.d,q=16+(r&3)*4,n=(r&32)!==0,l=b0+e.a,j=b1+e.b+1,d=(r&128)!==0,r=(r&64)!==0,k=0;k<64;++k){c=r?7-B.a.A(k,8):B.a.A(k,8)
b=d?7-B.a.u(k,8):B.a.u(k,8)
if(!(k<m.length))return A.a(m,k)
if(m[k]===0)continue
i=l+c
h=j+b
if(i<0||i>=a8||h<0||h>=a9)continue
a=h*a8+i
if(!g.i(0,a))continue
if(n&&a3.J(0,a))continue
if(!(k<m.length))return A.a(m,k)
a0=q+m[k]
if(!(a0>=0&&a0<32))return A.a(a1,a0)
a4.$3(i,h,a1[a0])}}},
l2(a,b){var s=new Uint8Array(432)
B.h.a7(s,0,432,b.f[0])
A.kM(a,b,s,24,18,4,0)
return s},
fU:function fU(a,b,c){this.a=a
this.b=b
this.c=c},
j1(a){var s=a/255
return s<=0.03928?s/12.92:A.ko(2.4*A.mX((s+0.055)/1.055))},
mX(a){var s,r,q
for(s=0,r=0;r<40;++r){q=A.ko(s)
s-=(q-a)/q}return s},
ko(a){var s,r,q
for(s=1,r=1,q=1;q<60;++q){s*=a/q
r+=s}return r},
kS(a){return 0.2126*A.j1(a>>>16)+0.7152*A.j1(a>>>8&255)+0.0722*A.j1(a&255)},
at(a,b){var s=A.kS(a),r=A.kS(b),q=s>r,p=q?s+0.05:r+0.05
return p/(q?r+0.05:s+0.05)},
aj(a,b,c){var s,r=new A.ho(a,b,c),q=r.$1(16)
if(typeof q!=="number")return q.ab()
s=r.$1(8)
if(typeof s!=="number")return s.ab()
r=r.$1(0)
if(typeof r!=="number")return A.nI(r)
return(q<<16|s<<8|r)>>>0},
e_(a,b,c){var s,r,q,p,o,n,m,l,k,j,i,h,g
if(A.at(a,b)>=c)return a
for(s=[16777215,0],r=a>>>16,q=a>>>8&255,p=a&255,o=null,n=0;n<2;++n){m=s[n]
for(l=1;l<=50;++l){k=l/50
j=A.aj(a,m,k)
if(A.at(j,b)>=c){if(o!=null){i=r-(o>>>16)
h=q-(o>>>8&255)
g=p-(o&255)
k=k<(i*i+h*h+g*g)/195075}else k=!0
if(k)o=j
break}}}if(o==null)s=A.at(16777215,b)>=A.at(0,b)?16777215:0
else s=o
return s},
jc(a,b,c,d){var s=A.aj(a,b,c),r=c
for(;;){if(!(A.at(d,s)<4.5&&r>0))break
s=A.aj(a,b,r)
r-=0.02}return s},
jB(a,b,c){var s
if(a===B.r){if(b==null){s=t.N
s=A.ac(s,s)}else s=A.n_(b,c)
return s}return A.n0(b==null?B.ad:b)},
n_(b5,b6){var s,r,q,p,o,n,m,l,k,j,i,h,g,f,e,d,c,b,a,a0,a1,a2,a3,a4,a5,a6,a7=16777215,a8="0",a9=b5.c,b0=b6?A.aj(a9,0,0.3):A.aj(a9,a7,0.86),b1=b6?A.aj(a9,a7,0.08):a7,b2=b6?A.aj(a9,0,0.45):a9,b3=b2,b4=1
for(;;){if(!(A.at(a7,b3)<4.5&&b4<=50))break
b3=A.aj(b2,0,b4/50);++b4}s=b6?15922682:857894
r=b6?463396:a7
q=new A.fj(b0)
p=b5.e
o=b5.d
if(!(A.at(p,o)>=4.5))p=A.at(a7,o)>=A.at(0,o)?a7:0
n=q.$1(b6?8119445:1407540)
m=B.c.n(B.a.k(o,16),6,a8)
l=B.c.n(B.a.k(p,16),6,a8)
k=B.c.n(B.a.k(b0,16),6,a8)
j=B.c.n(B.a.k(b1,16),6,a8)
i=B.c.n(B.a.k(b3,16),6,a8)
h=B.c.n(B.a.k(s,16),6,a8)
g=B.c.n(B.a.k(q.$1(b6?12174291:3951198),16),6,a8)
f=B.c.n(B.a.k(A.e_(A.aj(b0,s,0.3),b0,1.6),16),6,a8)
e=B.c.n(B.a.k(330518,16),6,a8)
d=B.c.n(B.a.k(r,16),6,a8)
c=B.c.n(B.a.k(q.$1(b6?o:a9),16),6,a8)
b=B.c.n(B.a.k(A.jc(b1,o,b6?0.3:0.2,s),16),6,a8)
a=B.c.n(B.a.k(A.jc(b1,n,0.22,s),16),6,a8)
a0=B.c.n(B.a.k(n,16),6,a8)
a1=B.c.n(B.a.k(n,16),6,a8)
a2=B.c.n(B.a.k(q.$1(b6?16765286:9067008),16),6,a8)
q=B.c.n(B.a.k(q.$1(b6?16743282:11740702),16),6,a8)
a3=B.c.n(B.a.k(o,16),6,a8)
a4=B.c.n(B.a.k(s,16),6,a8)
a5=b6?"radial-gradient(circle at 4% -2%, rgba(255, 253, 230, .5), transparent 30%), radial-gradient(circle at 96% -2%, rgba(255, 253, 230, .5), transparent 30%), linear-gradient(to bottom, #071733 0%, #0d2a55 62%, #123c26 100%)":"radial-gradient(circle at 85% -4%, rgba(255, 255, 255, .8), transparent 35%), linear-gradient(to bottom, #6fb7ff 0%, #a8d6ff 62%, #5fae5a 100%)"
a6=t.N
return A.df(["--accent","#"+m,"--accent-ink","#"+l,"--panel","#"+k,"--panel-hi","#"+j,"--panel-head","#"+i,"--ink","#"+h,"--muted","#"+g,"--line","#"+f,"--edge","#"+e,"--field","#"+d,"--headline","#"+c,"--sel","#"+b,"--copied","#"+a,"--copied-line","#"+a0,"--ok","#"+a1,"--warn","#"+a2,"--err","#"+q,"--canvas","#000000","--stripe","#"+a3,"--page-ink","#"+a4,"--page",a5,"--dots","rgba("+(o>>>16)+", "+(o>>>8&255)+", "+(o&255)+", 0.45)"],a6,a6)},
n0(a){var s,r,q,p,o,n,m,l=16777215,k="0",j=a.c,i=A.aj(j,l,0.1),h=j,g=1
for(;;){if(!(A.at(l,i)<7.2&&g<=50))break
h=A.aj(j,0,g/50)
i=A.aj(h,l,0.1);++g}s=A.e_(a.d,i,4.5)
r=B.c.n(B.a.k(h,16),6,k)
q=B.c.n(B.a.k(i,16),6,k)
p=B.c.n(B.a.k(A.e_(A.aj(l,h,0.4),i,4.5),16),6,k)
o=B.c.n(B.a.k(A.e_(A.aj(l,i,0.55),i,3),16),6,k)
n=B.c.n(B.a.k(s,16),6,k)
m=t.N
return A.df(["--bg","#"+r,"--panel","#"+q,"--ink","#ffffff","--muted","#"+p,"--line","#"+o,"--accent","#"+n,"--accent-ink","#"+B.c.n(B.a.k(A.at(l,s)>=A.at(0,s)?l:0,16),6,k),"--sel","#"+B.c.n(B.a.k(A.jc(i,s,0.25,l),16),6,k),"--stripe","#"+B.c.n(B.a.k(s,16),6,k),"--copied","#1e3323","--copied-line","#6fcf7a","--warn","#f0b35a","--err","#ff8a80","--ok","#6fcf7a","--canvas","#000000"],m,m)},
j9(a){var s,r,q=A.d([],t.s)
for(s=new A.N(a,A.A(a).h("N<1,2>")).gt(0);s.l();){r=s.d
q.push(r.a+":"+r.b)}return B.b.H(q,";")},
ox(a,b){if(a===B.t)return':root[data-style="nes"]{'+A.j9(A.jB(a,b,!1))+"}"
if(b==null)return""
return':root[data-style="modern"]{'+A.j9(A.jB(a,b,!1))+'}:root[data-style="modern"][data-mode="dark"]{'+A.j9(A.jB(a,b,!0))+"}"},
dA:function dA(a,b){this.a=a
this.b=b},
p:function p(a,b,c,d,e){var _=this
_.a=a
_.b=b
_.c=c
_.d=d
_.e=e},
ho:function ho(a,b,c){this.a=a
this.b=b
this.c=c},
fj:function fj(a){this.a=a},
nq(a){var s,r,q,p
for(s=a.length,r=4294967295,q=0;q<s;++q){p=a[q]
r=$.ll()[(r^p)&255]^r>>>8}return(r^4294967295)>>>0},
ot(a){var s,r,q,p,o,n,m,l,k=$.cY(),j=new A.bB(k),i=new A.bB(k),h=new A.iC(),g=new A.iD()
for(k=new A.N(a,A.A(a).h("N<1,2>")).gt(0);k.l();){s={}
r=k.d
r.toString
s.a=null
q=r.b
s.a=q
p=B.H.aL(r.a)
o=A.nq(q)
n=j.gm(0)
r=new A.iB(s,h,g,o,p)
g.$2(j,67324752)
r.$1(j)
j.i(0,p)
j.i(0,s.a)
g.$2(i,33639248)
h.$2(i,20)
r.$1(i)
h.$2(i,0)
h.$2(i,0)
h.$2(i,0)
g.$2(i,0)
g.$2(i,n)
i.i(0,p)}m=j.gm(0)
l=i.gm(0)
j.i(0,i.bx())
g.$2(j,101010256)
h.$2(j,0)
h.$2(j,0)
h.$2(j,a.a)
h.$2(j,a.a)
g.$2(j,l)
g.$2(j,m)
h.$2(j,0)
return j.bx()},
fg:function fg(){},
iC:function iC(){},
iD:function iD(){},
iB:function iB(a,b,c,d,e){var _=this
_.a=a
_.b=b
_.c=c
_.d=d
_.e=e},
jV(a){return new A.ep(a)},
e(a,b){var s=A.q(A.c(v.G.document).getElementById(a))
s.toString
return b.a(s)},
n(a,b){var s=A.I(new A.ht(b))
A.e(a,t.m).onclick=s
return s},
dW(){var s,r,q,p
for(s=[$.aC(),$.t()],r=v.G,q=0;q<2;++q){p=s[q]
A.c(A.q(A.c(r.document).getElementById(p.a+"Pane")).classList).remove("over")}},
nW(){var s,r,q,p,o,n,m,l,k,j,i,h=null,g="Attempting to rewrap a JS function."
for(s=[$.aC(),$.t()],r=A.aZ,q=v.G,p=0;p<2;++p){o=s[p]
n=o.a
m=A.q(A.c(q.document).getElementById(n+"File"))
m.toString
A.n(n+"Open",new A.h3(m))
A.n(n+"Logos",new A.h4(o))
l=new A.h5(m,o)
if(typeof l=="function")A.M(A.C(g,h))
k=function(a,b){return function(c){return a(b,c,arguments.length)}}(r,l)
j=$.a8()
k[j]=l
m.onchange=k
n=A.q(A.c(q.document).getElementById(n+"Pane"))
n.toString
m=new A.hf(n)
if(typeof m=="function")A.M(A.C(g,h))
k=function(a,b){return function(c){return a(b,c,arguments.length)}}(r,m)
k[j]=m
n.ondragover=k
m=new A.hg(n)
if(typeof m=="function")A.M(A.C(g,h))
k=function(a,b){return function(c){return a(b,c,arguments.length)}}(r,m)
k[j]=m
n.ondragleave=k
m=new A.hh(o)
if(typeof m=="function")A.M(A.C(g,h))
k=function(a,b){return function(c){return a(b,c,arguments.length)}}(r,m)
k[j]=m
n.ondrop=k}A.c(q.document).addEventListener("dragover",A.I(new A.hi()))
A.c(q.document).addEventListener("dragleave",A.I(new A.hj()))
A.c(q.document).addEventListener("drop",A.I(new A.hk()))
A.n("srcFromTarget",new A.hl())
A.n("undo",A.o8())
A.n("save",A.o7())
A.n("pvCancel",A.o3())
A.n("pvApply",A.nZ())
for(s=["optLogo","optColors","optShell"],p=0;p<3;++p){i=s[p]
n=A.q(A.c(q.document).getElementById(i))
n.toString
m=new A.hm()
if(typeof m=="function")A.M(A.C(g,h))
k=function(a,b){return function(c){return a(b,c,arguments.length)}}(r,m)
k[$.a8()]=m
n.onchange=k}A.n("ctxCopy",new A.h6())
A.n("ctxPaste",new A.h7())
A.n("ctxColors",new A.h8())
A.n("ceReset",new A.h9())
A.n("ceCancel",A.o1())
A.n("ceApply",A.nX())
A.n("ctxLogo",new A.ha())
A.oj()
A.ok()
A.ol()
A.oi()
A.c(q.document).addEventListener("mousedown",A.I(new A.hb()))
A.c(q.document).addEventListener("keydown",A.I(new A.hc()))
A.c(q.window).addEventListener("scroll",A.I(new A.hd()))
A.c(q.window).addEventListener("resize",A.I(new A.he()))
A.bT()},
au(){$.e7=$.cR=null
var s=t.m
A.e("ctx",s).hidden=!0
A.e("ctxS",s).hidden=!0},
cP(a,b){return A.nR(a,b)},
nR(a,b){var s=0,r=A.j6(t.H),q=1,p=[],o,n,m,l
var $async$cP=A.jd(function(c,d){if(c===1){p.push(d)
s=q}for(;;)switch(s){case 0:q=3
s=6
return A.iY(A.jt(A.c(b.arrayBuffer()),t.af),$async$cP)
case 6:o=d
A.l3(a,A.jU(o,0,null),A.B(b.name))
q=1
s=5
break
case 3:q=2
l=p.pop()
n=A.aQ(l)
A.e(a.a+"Info",t.m).textContent="Could not read "+A.B(b.name)+": "+A.k(n)
s=5
break
case 2:s=1
break
case 5:return A.j_(null,r)
case 1:return A.iZ(p.at(-1),r)}})
return A.j0($async$cP,r)},
l3(a,b,c){var s,r,q,p,o
try{q=A.bb(b)
s=new A.a1(q,A.aq(q))
s.G(0)
a.scd(s)
a.c=c
q=A.e(a.a+"Info",t.m)
p=s.b?"32-team":"original 28-team"
q.textContent=c+" \u2014 "+p+" ROM"}catch(o){r=A.aQ(o)
A.e(a.a+"Info",t.m).textContent="Could not use "+c+": "+A.k(r)
return}if(a===$.fR)$.ji=$.fR=$.bP=null
if(a===$.iy)$.jx=$.iy=$.e6=null
if(a===$.t()){$.cL.P(0)
$.cW.P(0)
B.b.P($.cX)
A.bK()
A.bL()
A.bN()}A.bM()
A.bT()},
bT(){var s,r,q,p,o=$.aC()
A.cJ(o)
s=$.t()
A.cJ(s)
r=t.m
A.e("undo",r).disabled=$.cX.length===0
A.e("save",r).disabled=s.b==null
A.e("srcFromTarget",r).disabled=s.b==null
for(o=[o,s],s=v.G,q=0;q<2;++q){p=o[q]
r=A.q(A.c(s.document).getElementById(p.a+"Logos"))
r.toString
r.disabled=p.b==null}},
cJ(a){var s,r,q,p,o,n,m,l,k,j,i,h,g,f,e="Attempting to rewrap a JS function.",d=a.b,c=A.e(a.a+"Grid",t.m)
c.innerHTML=""
A.dV(a)
if(d==null){p=A.c(A.c(v.G.document).createElement("div"))
p.className="empty"
p.textContent="No ROM loaded."
c.append(p)
return}s=0
p=A.aZ
o=v.G
n=t.s
for(;;){m=s
l=d.b?B.e:B.d
if(typeof m!=="number")return m.bA()
if(!(m<l.length))break
k=A.d(["team"],n)
if(a===$.fR&&J.S(s,$.ji))B.b.i(k,"copied")
if(a===$.t()&&J.S(s,$.cS))B.b.i(k,"pending")
j=A.c(A.c(o.document).createElement("div"))
j.className=B.b.H(k," ")
i=A.c(A.c(o.document).createElement("canvas"))
i.width=72
i.height=72
r=i
m=d.b?B.e:B.d
q=B.b.v(m,s)
try{A.b0(r,A.cT(d.a,d.G(s),!0))}catch(h){q=A.k(q)+" (error)"}j.append(r)
m=A.c(A.c(o.document).createElement("div"))
m.textContent=q
j.append(m)
if(a===$.t()&&$.cL.J(0,s)){m=A.c(A.c(o.document).createElement("span"))
m.className="dot"
m.setAttribute("title","changed")
j.append(m)}g=s
m=new A.fJ(a,g)
if(typeof m=="function")A.M(A.C(e,null))
f=function(b,a0){return function(a1){return b(a0,a1,arguments.length)}}(p,m)
l=$.a8()
f[l]=m
j.onclick=f
m=new A.fK(a,g)
if(typeof m=="function")A.M(A.C(e,null))
f=function(b,a0){return function(a1){return b(a0,a1,arguments.length)}}(p,m)
f[l]=m
j.oncontextmenu=f
c.append(j)
m=s
if(typeof m!=="number")return m.bz()
s=m+1}},
kI(a,b){var s,r,q=a.b
q.toString
$.bP=A.np(q,b)
$.fR=a
$.ji=b
s=$.t()
$.jh=a===s?s.c+" (target)":a.c
r=A.e("hint",t.m)
q=q.b?B.e:B.d
if(!(b<q.length))return A.a(q,b)
r.textContent="Copied "+q[b]+" from "+$.jh+". Right-click a target helmet \u2192 Paste helmet here."
A.cJ($.aC())
A.cJ(s)
if($.cS!=null)A.jv()},
kX(a){if($.bP==null){A.e("hint",t.m).textContent="Copy a helmet first (right-click \u2192 Copy helmet)."
return}A.bK()
A.bL()
A.bN()
$.cS=a
A.cJ($.t())
A.jv()},
jv(){var s,r,q,p,o,n,m,l,k,j,i,h,g,f,e,d,c=$.t().b,b=$.bP,a=$.cS
if(c==null||b==null||a==null)return
s=t.m
r=A.F(A.e("optLogo",s).checked)
q=A.F(A.e("optColors",s).checked)
p=A.F(A.e("optShell",s).checked)
o=$.js=A.l_(c,a,b,q,r,p)
A.e("preview",s).hidden=!1
n=A.e("pvTitle",s)
m=b.a
l=$.jh
k=c.b?B.e:B.d
if(a>>>0!==a||a>=k.length)return A.a(k,a)
n.textContent=m+" ("+l+") \u2192 "+k[a]
A.b0(A.e("pvBefore",s),A.cT(c.a,c.G(a),!0))
n=o.e
m=n!=null
if(m){n=A.bb(n)
j=new A.a1(n,A.aq(n))}else j=null
i=A.e("pvAfter",s)
if(j!=null)A.b0(i,A.cT(j.a,j.G(a),!0))
else A.q(i.getContext("2d")).clearRect(0,0,72,72)
n=A.d([],t.s)
for(l=o.a,k=l.length,h=0;h<l.length;l.length===k||(0,A.o)(l),++h){g=l[h]
f=A.a3(g,"&","&amp;")
f=A.a3(f,"<","&lt;")
n.push('<li class="err">'+A.a3(f,">","&gt;")+"</li>")}for(l=o.b,k=l.length,h=0;h<l.length;l.length===k||(0,A.o)(l),++h){e=l[h]
f=B.c.J(e,"isn't in this ROM")?"warn":"ok"
d=A.a3(e,"&","&amp;")
d=A.a3(d,"<","&lt;")
n.push('<li class="'+f+'">'+A.a3(d,">","&gt;")+"</li>")}l=o.c
if(l.length!==0)n.push('<li class="warn">Also changes '+A.e0(B.b.H(l,", "))+" (they share this logo).</li>")
l=!r
if(l&&!q&&!p)n.push('<li class="warn">Nothing selected to copy.</li>')
A.e("pvNotes",s).innerHTML=B.b.W(n)
n=A.e("pvApply",s)
if(m)m=l&&!q&&!p
else m=!0
n.disabled=m
A.e("preview",s).scrollIntoView()},
nj(){var s,r,q,p,o,n,m,l=$.t().b,k=$.js,j=$.cS
if(l==null||k==null||k.e==null||j==null)return
s=k.e
s.toString
r=A.eg([j],t.S)
for(q=k.c,p=q.length,o=l.b,n=0;n<q.length;q.length===p||(0,A.o)(q),++n){m=q[n]
r.i(0,B.b.a8(o?B.e:B.d,m))}q=$.bP.a
p=o?B.e:B.d
if(j>>>0!==j||j>=p.length)return A.a(p,j)
A.fQ(s,r,"Pasted "+q+" over "+p[j]+". Remember to save the target ROM.",B.p)
A.bM()
A.bT()},
fQ(a,b,c,d){var s=$.t(),r=t.S
B.b.i($.cX,new A.bE(new Uint8Array(A.z(s.b.a.a)),A.iP($.cL,r),A.iP($.cW,r)))
r=A.bb(a)
s.b=new A.a1(r,A.aq(r))
$.cL.C(0,b)
$.cW.C(0,d)
A.e("hint",t.m).textContent=c},
io(){var s,r,q,p,o,n,m,l,k,j,i,h,g,f,e,d,c,b,a="Attempting to rewrap a JS function.",a0=$.t().b,a1=$.dX
if(a0==null||a1==null)return
s=t.m
A.e("colorEd",s).hidden=!1
r=A.e("ceTitle",s)
q=a0.b?B.e:B.d
if(a1>>>0!==a1||a1>=q.length)return A.a(q,a1)
r.textContent=q[a1]+" colors"
A.b0(A.e("ceBefore",s),A.cT(a0.a,a0.G(a1),!0))
q=A.bb(A.jC(a0,a1,$.b_))
r=A.aq(q)
A.b0(A.e("ceAfter",s),A.cT(q,new A.a1(q,r).G(a1),!0))
p=A.e("ceSwatches",s)
p.innerHTML=""
for(r=t.t,r=[new A.i("Shell",A.d([0,1,2],r)),new A.i("Facemask",A.d([3,4],r)),new A.i("Logo",A.d([5,6,7],r))],q=A.aZ,o=v.G,n=0;n<3;++n){m=r[n]
l=m.b
k=A.c(A.c(o.document).createElement("div"))
k.className="swgroup"
j=A.c(A.c(o.document).createElement("span"))
j.textContent=m.a
k.append(j)
for(m=l.length,i=0;i<l.length;l.length===m||(0,A.o)(l),++i){h=l[i]
j=$.b_
if(!(h>=0&&h<j.length))return A.a(j,h)
g=j[h]
f=A.c(A.c(o.document).createElement("button"))
j=h===$.cM?"sw sel":"sw"
f.className=j
if(!(h<8))return A.a(B.o,h)
f.title=B.o[h]+": "+("$"+B.c.n(B.a.k(g,16),2,"0").toUpperCase())
f.textContent="$"+B.c.n(B.a.k(g,16),2,"0").toUpperCase()
j=A.c(f.style)
if(!(g>=0&&g<64))return A.a(B.f,g)
j.background="#"+B.c.n(B.a.k(B.f[g],16),6,"0")
e=A.jk(g)?"#000":"#fff"
j.color=e
e=new A.ip(h)
if(typeof e=="function")A.M(A.C(a,null))
d=function(a2,a3){return function(a4){return a2(a3,a4,arguments.length)}}(q,e)
d[$.a8()]=e
f.onclick=d
k.append(f)}p.append(k)}r=A.e("ceHelp",s)
m=$.cM
if(!(m>=0&&m<8))return A.a(B.o,m)
r.textContent="Choose a color for "+B.o[m]+"."
c=A.e("cePalette",s)
c.innerHTML=""
for(g=0;g<64;++g){f=A.c(A.c(o.document).createElement("button"))
r=$.b_
m=$.cM
if(!(m>=0&&m<r.length))return A.a(r,m)
r=g===r[m]?"sel":""
f.className=r
r=g===13
m=r?"$0D is unsafe (can upset TVs)":"$"+B.c.n(B.a.k(g,16),2,"0").toUpperCase()
f.title=m
A.c(f.style).background="#"+B.c.n(B.a.k(B.f[g],16),6,"0")
f.disabled=r
r=new A.iq(g)
if(typeof r=="function")A.M(A.C(a,null))
d=function(a2,a3){return function(a4){return a2(a3,a4,arguments.length)}}(q,r)
d[$.a8()]=r
f.onclick=d
c.append(f)}b=A.cO(a0,a1)
A.e("ceApply",s).disabled=A.dS(b,$.b_)
A.e("ceReset",s).disabled=A.dS(b,$.b_)},
dS(a,b){var s,r,q,p,o=a.length,n=J.iM(o,t.y)
for(s=a.length,r=b.length,q=0;q<o;++q){if(!(q<s))return A.a(a,q)
p=a[q]
if(!(q<r))return A.a(b,q)
n[q]=p===b[q]}return B.b.M(n,new A.fC())},
nh(){var s,r,q,p=$.t().b,o=$.dX
if(p==null||o==null)return
s=A.jC(p,o,$.b_)
r=A.eg([o],t.S)
q=p.b?B.e:B.d
if(o>>>0!==o||o>=q.length)return A.a(q,o)
A.fQ(s,r,"Changed "+q[o]+" colors. Remember to save the target ROM.",B.p)
A.bK()
A.bT()},
bK(){$.dX=null
A.e("colorEd",t.m).hidden=!0},
ns(a){if(!(a>=0&&a<64))return A.a(B.f,a)
return"#"+B.c.n(B.a.k(B.f[a],16),6,"0")},
jk(a){var s
if(!(a>=0&&a<64))return A.a(B.f,a)
s=B.f[a]
return 0.299*(s>>>16)+0.587*(s>>>8&255)+0.114*(s&255)>140},
oA(){var s,r,q=$.cX.length
if(q===0)return
if(0>=q)return A.a($.cX,-1)
s=$.cX.pop()
q=$.t()
r=A.bb(s.a)
q.b=new A.a1(r,A.aq(r))
$.cL.P(0)
$.cL.C(0,s.b)
$.cW.P(0)
$.cW.C(0,s.c)
A.bM()
A.bK()
A.bL()
A.bN()
A.bT()},
bM(){$.js=$.cS=null
A.e("preview",t.m).hidden=!0
var s=$.t()
if(s.b!=null)A.cJ(s)},
oh(){var s=$.t(),r=s.b
if(r==null)return
A.kL(r.a.a,A.kG(s.c)+"-helmets.nes","application/octet-stream")},
kG(a){var s=A.lT("\\s*\\(.*\\)$"),r=A.a3(a,s,""),q=B.c.cb(r,".")
return q>0?B.c.ac(r,0,q):r},
kL(a,b,c){var s=v.G,r=A.c(new s.Blob(A.d([a],t.G),{type:c})),q=A.B(s.URL.createObjectURL(r)),p=A.c(A.c(s.document).createElement("a"))
p.href=q
p.download=b
A.q(A.c(s.document).body).append(p)
p.click()
p.remove()
s.URL.revokeObjectURL(q)},
nv(a){var s,r,q,p,o,n,m,l=a.b
if(l==null)return
s=A.ac(t.N,t.p)
r=l.b
q=0
for(;;){if(!(q<(r?B.e:B.d).length))break
p=r?B.e:B.d
if(!(q<p.length))return A.a(p,q)
o=p[q]+"-logo.png"
for(n=2;s.aj(o);++n){p=r?B.e:B.d
if(!(q<p.length))return A.a(p,q)
o=p[q]+"-"+n+"-logo.png"}m=A.kR(A.jo(l,q),B.b.b0(A.cO(l,q),5))
s.q(0,o,B.y.aL(B.c.b1(m,B.c.a8(m,",")+1)));++q}A.kL(A.ot(s),A.kG(a.c)+"-logos.zip","application/zip")},
bJ(a){var s
A.B(a)
s=a.length
if(s===0)s=a
else{if(0>=s)return A.a(a,0)
s=a[0].toUpperCase()+B.c.b1(a,1)}return s},
e0(a){var s=A.a3(a,"&","&amp;")
s=A.a3(s,"<","&lt;")
return A.a3(s,">","&gt;")},
b0(a,b){var s=new Uint8ClampedArray(A.z(A.nL(b))),r=A.q(a.getContext("2d"))
r.toString
r.putImageData(A.c(new v.G.ImageData(s,A.j(a.width),A.j(a.height))),0,0)},
oj(){var s,r,q,p,o,n,m,l,k,j,i=t.m,h=A.e("leCanvas",i)
h.addEventListener("pointerdown",A.I(new A.hN(h)))
h.addEventListener("pointermove",A.I(new A.hO()))
s=new A.i0()
h.addEventListener("pointerup",A.I(s))
h.addEventListener("pointercancel",A.I(s))
h.addEventListener("contextmenu",A.I(new A.hP()))
A.n("tPencil",new A.hT())
A.n("tFill",new A.hU())
A.n("tPick",new A.hV())
A.n("tMove",new A.hW())
A.n("lePalBtn",new A.hX())
A.n("leArrange",new A.hY())
for(r=["leGuide","leBoxes"],q=A.aZ,p=v.G,o=0;o<2;++o){n=r[o]
m=A.q(A.c(p.document).getElementById(n))
m.toString
l=new A.hZ()
if(typeof l=="function")A.M(A.C("Attempting to rewrap a JS function.",null))
k=function(a,b){return function(c){return a(b,c,arguments.length)}}(q,l)
k[$.a8()]=l
m.onchange=k}A.n("leUndo",A.o9())
A.n("leClear",new A.i_())
A.n("leRevert",new A.hQ())
A.n("leExport",A.o5())
j=A.e("leFile",i)
A.n("leImport",new A.hR(j))
j.onchange=A.I(new A.hS(j))
A.n("leCancel",A.o2())
A.n("leApply",A.nY())},
nT(a){var s,r,q,p,o,n,m,l
if((A.F(a.ctrlKey)||A.F(a.metaKey))&&A.B(a.key).toLowerCase()==="z"){a.preventDefault()
A.l6()}else if(!A.F(a.ctrlKey)&&!A.F(a.metaKey)&&!A.F(a.altKey)){s=A.F(a.shiftKey)?8:1
r=-s
q=t.N
p=A.df(["ArrowLeft",new A.i(r,0),"ArrowRight",new A.i(s,0),"ArrowUp",new A.i(0,r),"ArrowDown",new A.i(0,s)],q,t.R).v(0,A.B(a.key))
o=$.an
n=$.aA
if(p!=null&&$.e3==="move"&&o!=null&&n!=null&&n<o.length){a.preventDefault()
A.bq()
m=A.kT($.a9(),o,n,p.a,p.b)
$.cQ=m.b
$.an=m.a
$.aA=0
A.az()
return}l=A.df(["p","pencil","f","fill","i","pick","m","move"],q,q).v(0,A.B(a.key).toLowerCase())
if(l!=null)A.cV(l)}},
bL(){$.jp=$.b1=null
$.e4=!1
$.dY=null
A.e("logoEd",t.m).hidden=!0},
cV(a){var s,r,q,p,o
$.e3=a
for(s=[B.a3,B.a1,B.a4,B.a2],r=v.G,q=0;q<4;++q){p=s[q]
o=A.q(A.c(r.document).getElementById(p.a))
o.toString
p=p.b===a?"on":""
o.className=p}s=A.c(A.e("leCanvas",t.m).style)
r=a==="move"?"move":"crosshair"
s.cursor=r
A.bQ()},
je(a,b){var s=A.c(A.e("leCanvas",t.m).getBoundingClientRect()),r=B.m.ak((A.a_(a.clientX)-A.a_(s.left))/A.a_(s.width)*72),q=B.m.ak((A.a_(a.clientY)-A.a_(s.top))/A.a_(s.height)*72)
if(b){r=B.a.V(r,0,71)
q=B.a.V(q,0,71)}return r>=0&&r<72&&q>=0&&q<72?new A.i(r,q):null},
oe(a,b){var s,r,q,p,o=a.a,n=a.b,m=b.a,l=b.b,k=Math.abs(m-o),j=-Math.abs(l-n),i=o<m?1:-1,h=n<l?1:-1,g=k+j
for(;;){s=$.a9()
r=n*72+o
q=$.jr
s.$flags&2&&A.r(s)
if(!(r>=0&&r<s.length))return A.a(s,r)
s[r]=q
if(o===m&&n===l)return
p=2*g
if(p>=j){g+=j
o+=i}if(p<=k){g+=k
n+=h}}},
nA(a,b){var s,r,q,p,o,n,m=$.a9(),l=a.b*72+a.a
if(!(l>=0&&l<m.length))return A.a(m,l)
s=m[l]
if(s===b)return
m=t.n
r=A.d([a],m)
while(l=r.length,l!==0){if(0>=l)return A.a(r,-1)
q=r.pop()
p=q.a
o=q.b
l=!0
if(p>=0)if(o>=0)if(p<72)if(o<72){l=$.a9()
n=o*72+p
if(!(n>=0&&n<l.length))return A.a(l,n)
n=l[n]!==s
l=n}if(l)continue
l=$.a9()
n=o*72+p
l.$flags&2&&A.r(l)
if(!(n>=0&&n<l.length))return A.a(l,n)
l[n]=b
B.b.C(r,A.d([new A.i(p+1,o),new A.i(p-1,o),new A.i(p,o+1),new A.i(p,o-1)],m))}},
bq(){var s,r=new Uint8Array(A.z($.a9())),q=$.an
if(q==null)q=null
else q=A.J(q,t.R)
s=A.J($.a0,t.S)
B.b.i($.bp,new A.bE(r,q,s))
if($.bp.length>200)B.b.ao($.bp,0)},
l6(){var s,r=$.bp.length
if(r===0)return
if(0>=r)return A.a($.bp,-1)
s=$.bp.pop()
$.cQ=s.a
$.an=s.b
$.a0=s.c
$.aA=null
A.az()},
az(){var s,r,q,p,o,n,m,l,k,j,i,h,g,f,e,d=$.t().b,c=$.b1
if(d==null||c==null)return
s=A.jf(d,c,$.a9(),$.an)
r=$.jn=s.a
q=$.an=r.e
p=$.aA
if(p!=null&&p>=q.length)$.aA=null
o=!A.dS($.a9(),$.iF())||!A.n7(q,$.kQ)
n=o?s.b:null
$.jp=n
q=A.dS($.a0,$.h2)
m=!q
p=t.m
l=A.e("leStats",p)
k=r.d
j=k===1?"":"s"
l.textContent=""+r.a+" of "+r.b+" sprites \xb7 busiest line "+r.c+" of 8 \xb7 "+k+" tile"+j
j=t.s
k=A.d([],j)
l=$.e1
if(l!=null)k.push('<li class="warn">'+A.e0(l)+"</li>")
l=n==null
if(l&&q)k.push("<li>No changes yet.</li>")
if(l&&m)k.push('<li class="ok">New colors. Only this team uses them.</li>')
if(!l){j=A.d([],j)
for(i=n.a,h=i.length,g=0;g<i.length;i.length===h||(0,A.o)(i),++g){f=i[g]
e=A.a3(f,"&","&amp;")
e=A.a3(e,"<","&lt;")
j.push('<li class="err">'+A.a3(e,">","&gt;")+"</li>")}if(s.c)j.push('<li class="warn">Auto-arrange sprites would make this fit.</li>')
if(n.e!=null)j.push('<li class="ok">Fits. '+A.e0(B.b.H(n.b," "))+"</li>")
i=n.c
if(i.length!==0)j.push('<li class="warn">Also changes '+A.e0(B.b.H(i,", "))+" (they share this logo).</li>")
B.b.C(k,j)}A.e("leNotes",p).innerHTML=B.b.W(k)
k=A.e("leApply",p)
l=l?q:n.e==null
k.disabled=l
A.e("leUndo",p).disabled=$.bp.length===0
p=A.e("leRevert",p)
q=!o&&q
p.disabled=q
A.iv()
A.bQ()},
iv(){var s,r,q,p,o,n,m,l,k,j,i,h,g="Attempting to rewrap a JS function.",f=t.m,e=A.e("leColors",f)
e.innerHTML=""
s=v.G
r=A.c(A.c(s.document).createElement("span"))
r.textContent="Color"
e.append(r)
for(r=A.aZ,q=0;q<4;++q){p=A.c(A.c(s.document).createElement("button"))
o=q===0
n=o?" clear":""
m=q===$.ao?" sel":""
p.className="sw"+n+m
n=o?"none":""+q
p.textContent=n
if(o)o="Transparent (right-click also erases)"
else{o=$.a0
n=q-1
if(!(n>=0&&n<o.length))return A.a(o,n)
n="Logo color "+q+": "+("$"+B.c.n(B.a.k(o[n],16),2,"0").toUpperCase())
o=n}p.title=o
if(q>0){o=A.c(p.style)
n=$.a0
m=q-1
if(!(m<n.length))return A.a(n,m)
n=n[m]
if(!(n>=0&&n<64))return A.a(B.f,n)
o.background="#"+B.c.n(B.a.k(B.f[n],16),6,"0")
n=$.a0
if(!(m<n.length))return A.a(n,m)
n=A.jk(n[m])?"#000":"#fff"
o.color=n}o=new A.iw(q)
if(typeof o=="function")A.M(A.C(g,null))
l=function(a,b){return function(c){return a(b,c,arguments.length)}}(r,o)
l[$.a8()]=o
p.onclick=l
e.append(p)}k=A.e("lePalBtn",f)
k.disabled=$.ao===0
o=$.e2?"Done changing colors":"Change color "+$.ao+"\u2026"
k.textContent=o
if($.ao===0)k.textContent="Change color\u2026 (pick color 1-3 first)"
j=A.e("lePalette",f)
i=!$.e2||$.ao===0
j.hidden=i
j.innerHTML=""
if($.ao===0)return
for(h=0;h<64;++h){p=A.c(A.c(s.document).createElement("button"))
f=$.a0
o=$.ao-1
if(!(o>=0&&o<f.length))return A.a(f,o)
f=h===f[o]?"sel":""
p.className=f
f=h===13
o=f?"$0D is unsafe (can upset TVs)":"$"+B.c.n(B.a.k(h,16),2,"0").toUpperCase()
p.title=o
A.c(p.style).background="#"+B.c.n(B.a.k(B.f[h],16),6,"0")
p.disabled=f
f=new A.ix(h)
if(typeof f=="function")A.M(A.C(g,null))
l=function(a,b){return function(c){return a(b,c,arguments.length)}}(r,f)
l[$.a8()]=f
p.onclick=l
j.append(p)}},
bQ(){var s,r,q,p,o,n,m,l,k,j,i,h,g,f,e,d=$.t().b,c=$.b1
if(d==null||c==null)return
s=d.G(c)
r=s.x
q=t.m
if(A.F(A.e("leGuide",q).checked))p=A.cT(d.a,s,!1)
else{p=new Uint8Array(5184)
if(0>=r.length)return A.a(r,0)
B.h.a7(p,0,5184,r[0])}for(o=p.length,n=p.$flags|0,m=0;m<o;++m){l=$.a9()
if(!(m<l.length))return A.a(l,m)
l=l[m]
if(l!==0){k=$.a0;--l
if(!(l>=0&&l<k.length))return A.a(k,l)
l=k[l]
n&2&&A.r(p)
p[m]=l}}o=$.ln()
A.b0(o,p)
n=A.q(A.e("leCanvas",q).getContext("2d"))
n.toString
n.imageSmoothingEnabled=!1
A.cK(n,"drawImage",[o,0,0,432,432],t.H)
j=$.an
if(j==null){o=$.jn
j=o==null?null:o.e}i=$.e3==="move"
if(j!=null)q=i||A.F(A.e("leBoxes",q).checked)
else q=!1
if(q){n.lineWidth=1
for(h=j.length-1;h>=0;--h){if(!(h<j.length))return A.a(j,h)
g=j[h]
f=i&&h===$.aA
e=f?"rgba(0, 230, 255, 1)":"rgba(255, 220, 0, 0.75)"
n.strokeStyle=e
q=f?3:1
n.lineWidth=q
n.strokeRect(g.a*6+0.5,g.b*6+0.5,47,47)}}},
n7(a,b){var s=a.length
return s===b.length&&A.lI(s,t.z).M(0,new A.fB(a,b))},
ni(){var s,r,q,p,o,n,m=$.t().b,l=$.b1,k=$.jp,j=A.dS($.a0,$.h2),i=!0
if(m!=null)if(l!=null){s=k==null
if(!(s&&j))i=!s&&k.e==null}if(i)return
i=k==null
r=i?null:k.e
if(r==null)r=m.a.a
if(!j){j=A.bb(r)
q=new A.a1(j,A.aq(j))
j=A.J(B.b.X(A.cO(q,l),0,5),t.S)
B.b.C(j,$.a0)
r=A.jC(q,l,j)}j=A.eg([l],t.S)
i=i?null:k.c
if(i==null)i=A.d([],t.s)
s=i.length
p=m.b
o=0
for(;o<i.length;i.length===s||(0,A.o)(i),++o){n=i[o]
j.i(0,B.b.a8(p?B.e:B.d,n))}i=p?B.e:B.d
if(l>>>0!==l||l>=i.length)return A.a(i,l)
A.fQ(r,j,"Changed "+i[l]+" logo. Remember to save the target ROM.",B.p)
A.bL()
A.bT()},
nw(){var s,r,q,p=$.t().b,o=$.b1
if(p==null||o==null)return
s=v.G
r=A.c(A.c(s.document).createElement("a"))
r.href=A.kR($.a9(),$.a0)
q=p.b?B.e:B.d
if(o>>>0!==o||o>=q.length)return A.a(q,o)
r.download=q[o]+"-logo.png"
A.q(A.c(s.document).body).append(r)
r.click()
r.remove()},
kR(a,b){var s,r=v.G,q=A.c(A.c(r.document).createElement("canvas"))
q.width=72
q.height=84
s=A.q(q.getContext("2d"))
s.toString
s.putImageData(A.c(new r.ImageData(new Uint8ClampedArray(A.z(A.nS(a,b))),72,84)),0,0)
return A.B(q.toDataURL("image/png"))},
fW(a){return A.nJ(a)},
nJ(a1){var s=0,r=A.j6(t.H),q,p=2,o=[],n,m,l,k,j,i,h,g,f,e,d,c,b,a,a0
var $async$fW=A.jd(function(a2,a3){if(a2===1){o.push(a3)
s=p}for(;;)switch(s){case 0:b=$.t().b
a=$.b1
if(b==null||a==null){s=1
break}p=4
e=v.G
s=7
return A.iY(A.jt(A.c(A.c(e.window).createImageBitmap(a1)),t.m),$async$fW)
case 7:n=a3
m=A.j(n.width)*84===A.j(n.height)*72?84:72
d=A.c(A.c(e.document).createElement("canvas"))
d.width=72
d.height=m
l=d
e=A.q(l.getContext("2d"))
e.toString
e.imageSmoothingEnabled=!1
k=e
A.cK(k,"drawImage",[n,0,0,72,m],t.H)
j=t.c.a(A.c(k.getImageData(0,0,72,m)).data)
i=A.of(new Uint8Array(A.z(j)),m,$.a0)
h=A.d([],t.s)
if(A.j(n.width)!==72||A.j(n.height)!==m)J.cZ(h,"The picture was "+A.j(n.width)+"\xd7"+A.j(n.height)+"; it was scaled to 72\xd7"+A.k(m)+".")
J.jE(h,i.c)
g=h
A.bq()
$.cQ=i.a
$.a0=i.b
$.an=null
$.e1=J.bt(g)===0?null:J.lr(g," ")
A.az()
p=2
s=6
break
case 4:p=3
a0=o.pop()
f=A.aQ(a0)
$.e1="Could not read "+A.B(a1.name)+": "+A.k(f)
A.az()
s=6
break
case 3:s=2
break
case 6:case 1:return A.j_(q,r)
case 2:return A.iZ(o.at(-1),r)}})
return A.j0($async$fW,r)},
dV(a0){var s,r,q,p,o,n,m,l,k,j,i,h,g,f,e,d,c="Attempting to rewrap a JS function.",b=a0.b,a=A.e(a0.a+"Small",t.m)
a.innerHTML=""
if(b==null)return
o=b.a
s=new A.bc(o,new A.a1(o,A.aq(o)))
r=0
o=A.aZ
n=v.G
m=t.s
for(;;){l=r
k=s.b.b?B.e:B.d
if(typeof l!=="number")return l.bA()
if(!(l<k.length))break
j=A.d(["team"],m)
if(a0===$.iy&&J.S(r,$.jx))B.b.i(j,"copied")
if(a0===$.t()&&J.S(r,$.bs))B.b.i(j,"pending")
i=A.c(A.c(n.document).createElement("div"))
i.className=B.b.H(j," ")
h=A.c(A.c(n.document).createElement("canvas"))
h.width=24
h.height=18
q=h
l=s.b.b?B.e:B.d
p=B.b.v(l,r)
try{l=b.a
k=s.G(r)
g=new Uint8Array(432)
B.h.a7(g,0,432,k.f[0])
A.kM(l,k,g,24,18,4,0)
A.b0(q,g)}catch(f){p=A.k(p)+" (error)"}i.append(q)
l=A.c(A.c(n.document).createElement("div"))
l.textContent=p
i.append(l)
if(a0===$.t()&&$.cW.J(0,r)){l=A.c(A.c(n.document).createElement("span"))
l.className="dot"
l.setAttribute("title","changed")
i.append(l)}e=r
l=new A.fL(a0,e)
if(typeof l=="function")A.M(A.C(c,null))
d=function(a1,a2){return function(a3){return a1(a2,a3,arguments.length)}}(o,l)
k=$.a8()
d[k]=l
i.onclick=d
l=new A.fM(a0,e)
if(typeof l=="function")A.M(A.C(c,null))
d=function(a1,a2){return function(a3){return a1(a2,a3,arguments.length)}}(o,l)
d[k]=l
i.oncontextmenu=d
a.append(i)
l=r
if(typeof l!=="number")return l.bz()
r=l+1}},
kJ(a,b){var s,r,q=a.b,p=A.k0(q.a,b)
$.e6=new A.eD(p.d.a,p.ga5(),p.T().a,p.f.length!==0,A.eh(p.gY(),t.S))
$.iy=a
$.jx=b
p=$.t()
$.jw=a===p?p.c+" (target)":a.c
s=A.e("hint",t.m)
r=q.b?B.e:B.d
if(!(b<r.length))return A.a(r,b)
s.textContent="Copied "+r[b]+"'s small helmet from "+$.jw+". Right-click a target small helmet \u2192 Paste small helmet here."
A.dV($.aC())
A.dV(p)},
kY(a){var s=$.e6
if(s==null){A.e("hint",t.m).textContent="Copy a small helmet first (right-click \u2192 Copy small helmet)."
return}A.kV(a,s)},
ok(){var s,r,q,p,o,n,m,l,k,j,i,h
A.n("ctxSCopy",new A.i1())
A.n("ctxSPaste",new A.i2())
A.n("ctxSEdit",new A.i3())
s=t.m
r=A.e("seCanvas",s)
r.addEventListener("pointerdown",A.I(new A.i8(r)))
r.addEventListener("pointermove",A.I(new A.i9()))
q=new A.ih()
r.addEventListener("pointerup",A.I(q))
r.addEventListener("pointercancel",A.I(q))
r.addEventListener("contextmenu",A.I(new A.ia()))
A.n("sLayerBg",new A.ib())
A.n("sLayerOv",new A.ic())
A.n("sPencil",new A.id())
A.n("sFill",new A.ie())
A.n("sPick",new A.ig())
for(p=["seGuide","seGrid"],o=A.aZ,n=v.G,m=0;m<2;++m){l=p[m]
k=A.q(A.c(n.document).getElementById(l))
k.toString
j=new A.i4()
if(typeof j=="function")A.M(A.C("Attempting to rewrap a JS function.",null))
i=function(a,b){return function(c){return a(b,c,arguments.length)}}(o,j)
i[$.a8()]=j
k.onchange=i}A.n("seUndo",A.oa())
A.n("seRevert",new A.i5())
A.n("seExport",A.o6())
h=A.e("seFile",s)
A.n("seImport",new A.i6(h))
h.onchange=A.I(new A.i7(h))
A.n("seCancel",A.o4())
A.n("seApply",A.o_())},
kV(a,b){var s,r,q,p,o=$.t(),n=o.b
if(n==null)return
A.bM()
A.bK()
A.bL()
s=A.k0(n.a,a)
$.bs=a
$.L=s
B.b.P($.b3)
$.a2="bg"
$.br=A.d([],t.s)
r=n.b
q=r?B.e:B.d
if(!(a<q.length))return A.a(q,a)
$.jA=q[a]+" small helmet"
if(b!=null){$.br=s.c3(b).c
q=b.a
p=$.jw
r=r?B.e:B.d
if(!(a<r.length))return A.a(r,a)
$.jA=q+" ("+p+") \u2192 "+r[a]+" small helmet"}r=t.m
A.e("smallEd",r).hidden=!1
A.cU($.iA,!1)
A.ju("bg",!1)
A.dV(o)
A.aB()
A.e("smallEd",r).scrollIntoView()},
bN(){var s,r=$.bs
$.L=$.bs=null
s=$.e8=!1
A.e("smallEd",t.m).hidden=!0
if(r!=null?$.t().b!=null:s)A.dV($.t())},
ju(a,b){var s,r,q
$.a2=a
s=t.m
r=A.e("sLayerBg",s)
q=a==="bg"?"on":""
r.className=q
s=A.e("sLayerOv",s)
r=a==="overlay"?"on":""
s.className=r
if(b)A.aB()},
cU(a,b){var s,r,q,p,o
$.iA=a
for(s=[B.a_,B.Z,B.a0],r=v.G,q=0;q<3;++q){p=s[q]
o=A.q(A.c(r.document).getElementById(p.a))
o.toString
p=p.b===a?"on":""
o.className=p}if(b)A.dZ()},
on(a,b){var s=$.a2,r=$.L
return s==="bg"?r.O(a,b):r.aS(a,b)},
oq(a,b,c){var s=$.a2,r=$.L
return s==="bg"?r.am(a,b,c):r.an(a,b,c)},
l5(a,b){var s=A.c(A.e("seCanvas",t.m).getBoundingClientRect()),r=$.a2==="bg",q=r?16:24,p=r?16:18,o=B.m.ak((A.a_(a.clientX)-A.a_(s.left))/A.a_(s.width)*q),n=B.m.ak((A.a_(a.clientY)-A.a_(s.top))/A.a_(s.height)*p)
if(b){o=B.a.V(o,0,q-1)
n=B.a.V(n,0,p-1)}return o>=0&&o<q&&n>=0&&n<p?new A.i(o,n):null},
or(a,b){var s,r,q,p,o=a.a,n=a.b,m=b.a,l=b.b,k=Math.abs(m-o),j=-Math.abs(l-n),i=o<m?1:-1,h=n<l?1:-1,g=k+j
for(;;){s=$.jz
r=$.a2
q=$.L
if(r==="bg")q.am(o,n,s)
else q.an(o,n,s)
if(o===m&&n===l)return
p=2*g
if(p>=j){g+=j
o+=i}if(p<=k){g+=k
n+=h}}},
oo(a,b){var s,r,q,p,o,n,m,l,k,j=$.a2==="bg",i=j?16:24,h=j?16:18,g=$.L,f=new Uint8Array(A.z(j?g.ga5():g.T().a))
g=a.b*i+a.a
s=f.length
if(!(g>=0&&g<s))return A.a(f,g)
r=f[g]
if(r===b)return
j=i*h
q=A.aU(j,!1,!1,t.y)
g=t.n
p=A.d([a],g)
while(o=p.length,o!==0){if(0>=o)return A.a(p,-1)
n=p.pop()
m=n.a
l=n.b
o=!0
if(m>=0)if(l>=0)if(m<i)if(l<h){k=l*i+m
if(!(k>=0&&k<j))return A.a(q,k)
if(!q[k]){if(!(k<s))return A.a(f,k)
if(f[k]===r){o=$.a2
k=$.L
k=!(o==="bg"?k.O(m,l):k.aS(m,l))
o=k}}}if(o)continue
B.b.q(q,l*i+m,!0)
o=$.a2
k=$.L
if(o==="bg")k.am(m,l,b)
else k.an(m,l,b)
B.b.C(p,A.d([new A.i(m+1,l),new A.i(m-1,l),new A.i(m,l+1),new A.i(m,l-1)],g))}},
hx(){B.b.i($.b3,$.L.bC())
if($.b3.length>200)B.b.ao($.b3,0)},
l7(){var s=$.L
if(s==null||$.b3.length===0)return
if(0>=$.b3.length)return A.a($.b3,-1)
s.bw($.b3.pop())
A.aB()},
aB(){var s,r,q,p,o,n,m,l,k=$.t().b,j=$.L,i=$.bs
if(k==null||j==null||i==null)return
s=t.m
A.e("seTitle",s).textContent=$.jA
r=j.gbl()
q=A.d([],t.s)
for(p=$.br,o=p.length,n=0;n<p.length;p.length===o||(0,A.o)(p),++n){m=p[n]
l=A.a3(m,"&","&amp;")
l=A.a3(l,"<","&lt;")
q.push('<li class="warn">'+A.a3(l,">","&gt;")+"</li>")}if(!(j.gS().a!==0||j.gZ())&&$.br.length===0)q.push("<li>No changes yet.</li>")
if((j.gS().a!==0||j.gZ())&&r.length===0)q.push('<li class="ok">Only this helmet changes.</li>')
if(r.length!==0){p=A.Q(r)
q.push('<li class="warn">Also changes '+A.e0(new A.ae(r,p.h("P(1)").a(A.o0()),p.h("ae<1,P>")).H(0,", "))+" (they use the same tiles).</li>")}if(j.f.length===0&&$.a2==="overlay")q.push("<li>This helmet has no overlay sprites.</li>")
if($.a2==="bg"&&!j.O(0,0)&&!j.O(8,0)&&!j.O(0,8)&&!j.O(8,8))q.push("<li>This helmet has no background tiles.</li>")
A.e("seNotes",s).innerHTML=B.b.W(q)
q=A.e("seApply",s)
q.disabled=!(j.gS().a!==0||j.gZ())
A.e("seUndo",s).disabled=$.b3.length===0
q=A.e("seRevert",s)
q.disabled=!(j.gS().a!==0||j.gZ())
A.e("sLayerOv",s).disabled=!1
A.l4()
A.om()
A.dZ()},
l4(){var s,r,q,p,o,n,m,l,k,j,i,h,g,f,e,d,c,b,a,a0,a1="Attempting to rewrap a JS function.",a2=$.L
a2.toString
if($.a2==="bg"){s=a2.gY()
if(0>=s.length)return A.a(s,0)
r=s[0]*4}else{s=a2.ga9().length===0?0:B.b.ga0($.L.ga9()).a
r=16+a2.bu(s,$.L.ga9().length===0?0:B.b.ga0($.L.ga9()).b)*4}s=t.m
q=A.e("seColors",s)
q.innerHTML=""
p=v.G
o=A.c(A.c(p.document).createElement("span"))
o.textContent="Color"
q.append(o)
for(o=A.aZ,n=a2.d.f,m=0;m<4;++m){l=r+m
if(!(l>=0&&l<32))return A.a(n,l)
k=n[l]
j=A.c(A.c(p.document).createElement("button"))
l=m===0
i=l?" clear":""
h=m===$.e5?" sel":""
j.className="sw"+i+h
i=l?"none":""+m
j.textContent=i
l=l?"Clear (right-click also erases)":"Color "+m+": "+("$"+B.c.n(B.a.k(k,16),2,"0").toUpperCase())
j.title=l
if(m>0){l=A.c(j.style)
if(!(k<64))return A.a(B.f,k)
l.background="#"+B.c.n(B.a.k(B.f[k],16),6,"0")
i=A.jk(k)?"#000":"#fff"
l.color=i}l=new A.is(m)
if(typeof l=="function")A.M(A.C(a1,null))
g=function(a3,a4){return function(a5){return a3(a4,a5,arguments.length)}}(o,l)
g[$.a8()]=l
j.onclick=g
q.append(j)}f=A.e("sePalGroups",s)
f.innerHTML=""
s=A.c(A.c(p.document).createElement("span"))
s.textContent="Palette group"
f.append(s)
e=A.eh(a2.gY(),t.S)
d=B.b.M(e,new A.it(e))
for(s=t.s,l=e.length,c=0;c<4;c=a0){i=A.d([],s)
for(h=c*4,m=1;m<4;++m){b=h+m
if(!(b<32))return A.a(n,b)
b=n[b]
if(!(b<64))return A.a(B.f,b)
i.push('<i style="background:'+("#"+B.c.n(B.a.k(B.f[b],16),6,"0"))+'"></i>')}a=B.b.W(i)
j=A.c(A.c(p.document).createElement("button"))
if(d){if(0>=l)return A.a(e,0)
i=e[0]===c}else i=!1
j.className="sw"+(i?" sel":"")
a0=c+1
j.title="Use color group "+a0+" (shared by every helmet that uses it)"
j.innerHTML='<span class="sechips">'+a+"</span>"
A.c(j.style).width="64px"
h=new A.iu(a2,c)
if(typeof h=="function")A.M(A.C(a1,null))
g=function(a3,a4){return function(a5){return a3(a4,a5,arguments.length)}}(o,h)
g[$.a8()]=h
j.onclick=g
f.append(j)}if(!d){a2=A.c(A.c(p.document).createElement("span"))
a2.className="muted"
a2.textContent="This helmet's left and right halves use different groups; picking one sets all four tiles."
f.append(a2)}},
om(){var s,r,q,p,o=$.t().b,n=$.L,m=$.bs
if(o==null||n==null||m==null)return
s=t.m
r=A.e("seBefore",s)
q=o.a
A.b0(r,A.l2(q,new A.bc(q,new A.a1(q,A.aq(q))).G(m)))
p=A.bb(n.bp())
A.b0(A.e("seAfter",s),A.l2(p,new A.bc(p,new A.a1(p,A.aq(p))).G(m)))},
od(a){var s,r,q,p,o=a.length,n=o*4,m=new Uint8ClampedArray(n)
for(s=0;s<o;++s){r=a[s]
if(r===255)continue
q=B.f[r&63]
r=s*4
if(!(r<n))return A.a(m,r)
m[r]=q>>>16
p=r+1
if(!(p<n))return A.a(m,p)
m[p]=q>>>8&255
p=r+2
if(!(p<n))return A.a(m,p)
m[p]=q&255
r+=3
if(!(r<n))return A.a(m,r)
m[r]=255}return m},
hs(a,b,c){var s,r=v.G,q=A.c(A.c(r.document).createElement("canvas"))
q.width=b
q.height=c
s=A.q(q.getContext("2d"))
s.toString
s.putImageData(A.c(new r.ImageData(A.od(a),b,c)),0,0)
return q},
dZ(){var s,r,q,p,o,n,m,l,k,j,i,h,g,f,e="drawImage",d=$.L
if(d==null)return
s=$.a2==="bg"
r=s?16:24
q=s?16:18
p=s?20:14
s=t.m
o=A.e("seCanvas",s)
n=r*p
if(A.j(o.width)!==n)o.width=n
m=q*p
if(A.j(o.height)!==m)o.height=m
l=A.q(o.getContext("2d"))
l.toString
l.imageSmoothingEnabled=!1
l.globalAlpha=1
l.fillStyle=A.ns(d.d.f[0])
l.fillRect(0,0,n,m)
k=$.a2==="bg"
if(A.F(A.e("seGuide",s).checked)){l.globalAlpha=0.35
j=t.H
if(k)A.cK(l,e,[A.hs(d.aT(),24,18),-4*p,-p,24*p,18*p],j)
else{i=16*p
A.cK(l,e,[A.hs(d.aH(),16,16),4*p,p,i,i],j)}l.globalAlpha=1}A.cK(l,e,[k?A.hs(d.aH(),r,q):A.hs(d.aT(),r,q),0,0,n,m],t.H)
l.fillStyle="rgba(0, 0, 0, 0.5)"
for(h=0;h<q;++h)for(n=h*p,g=0;g<r;++g){m=$.a2
j=$.L
if(!(m==="bg"?j.O(g,h):j.aS(g,h)))l.fillRect(g*p,n,p,p)}if(A.F(A.e("seGrid",s).checked)){l.lineWidth=1
if(k){l.strokeStyle="rgba(255, 220, 0, 0.75)"
for(s=[B.V,B.X,B.W,B.Y],n=8*p-1,f=0;f<4;++f){m=s[f]
l.strokeRect(m.a*p+0.5,m.b*p+0.5,n,n)}}else{l.strokeStyle="rgba(255, 220, 0, 0.75)"
for(s=d.ga9(),n=s.length,m=8*p-1,f=0;f<s.length;s.length===n||(0,A.o)(s),++f){j=s[f]
l.strokeRect(j.a*p+0.5,j.b*p+0.5,m,m)}}}},
nk(){var s,r,q,p,o,n,m=$.t().b,l=$.L,k=$.bs,j=!0
if(m!=null)if(l!=null)if(k!=null)j=!(l.gS().a!==0||l.gZ())
if(j)return
j=t.S
s=A.a4(j)
for(r=l.gbl(),q=r.length,p=m.b,o=0;o<r.length;r.length===q||(0,A.o)(r),++o){n=r[o]
s.i(0,B.b.a8(p?B.e:B.d,n))}r=l.bp()
q=p?B.e:B.d
if(k>>>0!==k||k>=q.length)return A.a(q,k)
q=q[k]
p=A.eg([k],j)
p.C(0,s)
A.fQ(r,A.a4(j),"Changed "+q+"'s small helmet. Remember to save the target ROM.",p)
A.bN()
A.bT()},
nx(){var s,r,q,p,o=$.L
if(o==null)return
s=v.G
r=A.c(A.c(s.document).createElement("canvas"))
r.width=24
r.height=36
q=A.q(r.getContext("2d"))
q.toString
q.putImageData(A.c(new s.ImageData(new Uint8ClampedArray(A.z(A.op(o))),24,36)),0,0)
p=A.c(A.c(s.document).createElement("a"))
p.href=A.B(r.toDataURL("image/png"))
p.download=o.d.a+"-small-helmet.png"
A.q(A.c(s.document).body).append(p)
p.click()
p.remove()},
fX(a){return A.nK(a)},
nK(a1){var s=0,r=A.j6(t.H),q,p=2,o=[],n,m,l,k,j,i,h,g,f,e,d,c,b,a,a0
var $async$fX=A.jd(function(a2,a3){if(a2===1){o.push(a3)
s=p}for(;;)switch(s){case 0:a=$.L
if(a==null){s=1
break}p=4
d=v.G
s=7
return A.iY(A.jt(A.c(A.c(d.window).createImageBitmap(a1)),t.m),$async$fX)
case 7:n=a3
if(A.j(n.width)*36!==A.j(n.height)*24){$.br=A.d(["The picture is "+A.j(n.width)+"\xd7"+A.j(n.height)+"; it must be 24\xd736 (or a multiple of that, like an exported picture scaled up)."],t.s)
A.aB()
s=1
break}c=A.c(A.c(d.document).createElement("canvas"))
c.width=24
c.height=36
m=c
d=A.q(m.getContext("2d"))
d.toString
d.imageSmoothingEnabled=!1
l=d
A.cK(l,"drawImage",[n,0,0,24,36],t.H)
k=t.c.a(A.c(l.getImageData(0,0,24,36)).data)
j=A.og(new Uint8Array(A.z(k)),a)
A.hx()
i=A.d([],t.s)
if(A.j(n.width)!==24)J.cZ(i,"The picture was "+A.j(n.width)+"\xd7"+A.j(n.height)+"; it was scaled to 24\xd736.")
J.jE(i,j.c)
h=i
g=a.bm(j.a)
f=a.bn(j.b)
i=g
if(typeof i!=="number"){q=i.aY()
s=1
break}if(i>0){i=A.k(g)
d=J.S(g,1)?"":"s"
J.cZ(h,i+" background pixel"+d+" had no tile to go in and were left out.")}i=f
if(typeof i!=="number"){q=i.aY()
s=1
break}if(i>0){i=A.k(f)
d=J.S(f,1)?"":"s"
J.cZ(h,i+" overlay pixel"+d+" were outside every sprite and were left out.")}$.br=h
A.aB()
p=2
s=6
break
case 4:p=3
a0=o.pop()
e=A.aQ(a0)
$.br=A.d(["Could not read "+A.B(a1.name)+": "+A.k(e)],t.s)
A.aB()
s=6
break
case 3:s=2
break
case 6:case 1:return A.j_(q,r)
case 2:return A.iZ(o.at(-1),r)}})
return A.j0($async$fX,r)},
ja(a){var s,r
try{s=A.iX(A.c(A.c(v.G.window).localStorage).getItem(a))
return s}catch(r){return null}},
dT(a,b){var s
try{A.c(A.c(v.G.window).localStorage).setItem(a,b)}catch(s){}},
ol(){var s,r,q,p,o,n=t.m,m=A.e("styleSel",n),l=A.e("teamSel",n),k=A.e("modeSel",n)
n=v.G
s=A.c(A.c(n.document).createElement("option"))
s.value=""
s.textContent="Default"
l.append(s)
for(r=0;r<32;++r){q=B.q[r]
s=A.c(A.c(n.document).createElement("option"))
s.value=q.a
s.textContent=q.b
l.append(s)}s=A.ja("tsb.style")==="nes"?"nes":"modern"
m.value=s
p=A.ja("tsb.team")
if(p==null)p=""
s=B.b.K(B.q,new A.ii(p))?p:""
l.value=s
o=A.ja("tsb.mode")
if(o==="light"||o==="dark"){o.toString
s=o}else s="auto"
k.value=s
m.onchange=A.I(new A.ij())
l.onchange=A.I(new A.ik())
k.onchange=A.I(new A.il())
A.c(A.c(n.window).matchMedia("(prefers-color-scheme: dark)")).addEventListener("change",A.I(new A.im()))
A.dU()},
dU(){var s,r,q,p,o,n,m,l,k,j,i=t.m,h=A.B(A.e("styleSel",i).value)==="nes"?B.t:B.r,g=A.B(A.e("teamSel",i).value),f=A.lH(new A.aL(B.q,t.ax.a(new A.fH(g)),t.aA),t.Q),e=A.ox(h,f),d=v.G,c=A.q(A.c(d.document).documentElement)
c.toString
s=h===B.t
r=s?"nes":"modern"
c.setAttribute("data-style",r)
q=h===B.r&&f!=null
if(q)c.setAttribute("data-skin","team")
else c.removeAttribute("data-skin")
p=A.e("modeSel",i)
o=A.F(A.c(A.c(d.window).matchMedia("(prefers-color-scheme: dark)")).matches)
if(A.B(p.value)!=="dark")n=A.B(p.value)==="auto"&&o
else n=!0
r=n?"dark":"light"
c.setAttribute("data-mode",r)
p.disabled=s
c=s?"The NES style is always dark":"Light, dark, or follow your computer"
p.title=c
A.e("themeStyle",i).textContent=e
A.dT("tsb.style",s?"nes":"modern")
A.dT("tsb.team",g)
A.dT("tsb.mode",A.B(p.value))
A.dT("tsb.skin",q?"team":"")
A.dT("tsb.css",e)
m=A.e("teamChips",i)
m.innerHTML=""
i=f==null?B.T:A.d([f.c,f.d],t.t)
c=i.length
l=0
for(;l<i.length;i.length===c||(0,A.o)(i),++l){k=i[l]
j=A.c(A.c(d.document).createElement("i"))
j.setAttribute("style","background:#"+B.c.n(B.a.k(k,16),6,"0"))
m.append(j)}},
oi(){var s=A.e("helpDlg",t.m)
A.n("helpBtn",new A.hJ(s))
A.n("helpClose",new A.hK(s))
s.addEventListener("click",A.I(new A.hL(s)))},
ep:function ep(a){this.a=a
this.b=null
this.c=""},
ht:function ht(a){this.a=a},
h3:function h3(a){this.a=a},
h4:function h4(a){this.a=a},
h5:function h5(a,b){this.a=a
this.b=b},
hf:function hf(a){this.a=a},
hg:function hg(a){this.a=a},
hh:function hh(a){this.a=a},
hi:function hi(){},
hj:function hj(){},
hk:function hk(){},
hl:function hl(){},
hm:function hm(){},
h6:function h6(){},
h7:function h7(){},
h8:function h8(){},
h9:function h9(){},
ha:function ha(){},
hb:function hb(){},
hc:function hc(){},
hd:function hd(){},
he:function he(){},
fJ:function fJ(a,b){this.a=a
this.b=b},
fK:function fK(a,b){this.a=a
this.b=b},
ip:function ip(a){this.a=a},
iq:function iq(a){this.a=a},
fC:function fC(){},
hN:function hN(a){this.a=a},
hM:function hM(a){this.a=a},
hO:function hO(){},
i0:function i0(){},
hP:function hP(){},
hT:function hT(){},
hU:function hU(){},
hV:function hV(){},
hW:function hW(){},
hX:function hX(){},
hY:function hY(){},
hZ:function hZ(){},
i_:function i_(){},
hQ:function hQ(){},
hR:function hR(a){this.a=a},
hS:function hS(a){this.a=a},
iw:function iw(a){this.a=a},
ix:function ix(a){this.a=a},
fB:function fB(a,b){this.a=a
this.b=b},
fL:function fL(a,b){this.a=a
this.b=b},
fM:function fM(a,b){this.a=a
this.b=b},
i1:function i1(){},
i2:function i2(){},
i3:function i3(){},
i8:function i8(a){this.a=a},
i9:function i9(){},
ih:function ih(){},
ia:function ia(){},
ib:function ib(){},
ic:function ic(){},
id:function id(){},
ie:function ie(){},
ig:function ig(){},
i4:function i4(){},
i5:function i5(){},
i6:function i6(a){this.a=a},
i7:function i7(a){this.a=a},
is:function is(a){this.a=a},
it:function it(a){this.a=a},
iu:function iu(a,b){this.a=a
this.b=b},
ir:function ir(a){this.a=a},
ii:function ii(a){this.a=a},
ij:function ij(){},
ik:function ik(){},
il:function il(){},
im:function im(){},
fH:function fH(a){this.a=a},
hJ:function hJ(a){this.a=a},
hK:function hK(a){this.a=a},
hL:function hL(a){this.a=a},
oy(a){throw A.U(new A.de("Field '"+a+"' has been assigned during initialization."),new Error())}},B={}
var w=[A,J,B]
var $={}
A.iN.prototype={}
J.d9.prototype={
N(a,b){return a===b},
gD(a){return A.ds(a)},
j(a){return"Instance of '"+A.dt(a)+"'"},
gE(a){return A.bm(A.j3(this))}}
J.db.prototype={
j(a){return String(a)},
gD(a){return a?519018:218159},
gE(a){return A.bm(t.y)},
$iy:1,
$im:1}
J.c4.prototype={
N(a,b){return null==b},
j(a){return"null"},
gD(a){return 0},
$iy:1}
J.c6.prototype={$iE:1}
J.aT.prototype={
gD(a){return 0},
j(a){return String(a)}}
J.dr.prototype={}
J.bd.prototype={}
J.aF.prototype={
j(a){var s=a[$.a8()]
if(s==null)return this.bE(a)
return"JavaScript function for "+J.d_(s)},
$ib4:1}
J.bw.prototype={
gD(a){return 0},
j(a){return String(a)}}
J.bx.prototype={
gD(a){return 0},
j(a){return String(a)}}
J.u.prototype={
i(a,b){A.Q(a).c.a(b)
a.$flags&1&&A.r(a,29)
a.push(b)},
ao(a,b){a.$flags&1&&A.r(a,"removeAt",1)
if(b<0||b>=a.length)throw A.f(A.jZ(b,null))
return a.splice(b,1)[0]},
aV(a,b){var s
a.$flags&1&&A.r(a,"remove",1)
for(s=0;s<a.length;++s)if(J.S(a[s],b)){a.splice(s,1)
return!0}return!1},
C(a,b){var s
A.Q(a).h("h<1>").a(b)
a.$flags&1&&A.r(a,"addAll",2)
if(Array.isArray(b)){this.bJ(a,b)
return}for(s=J.bU(b);s.l();)a.push(s.gp())},
bJ(a,b){var s,r
t.b.a(b)
s=b.length
if(s===0)return
if(a===b)throw A.f(A.Z(a))
for(r=0;r<s;++r)a.push(b[r])},
P(a){a.$flags&1&&A.r(a,"clear","clear")
a.length=0},
bt(a,b,c){var s=A.Q(a)
return new A.ae(a,s.F(c).h("1(2)").a(b),s.h("@<1>").F(c).h("ae<1,2>"))},
H(a,b){var s,r=A.aU(a.length,"",!1,t.N)
for(s=0;s<a.length;++s)this.q(r,s,A.k(a[s]))
return r.join(b)},
W(a){return this.H(a,"")},
I(a,b){if(!(b>=0&&b<a.length))return A.a(a,b)
return a[b]},
X(a,b,c){var s=a.length
if(b>s)throw A.f(A.ar(b,0,s,"start",null))
if(c==null)c=s
else if(c<b||c>s)throw A.f(A.ar(c,b,s,"end",null))
if(b===c)return A.d([],A.Q(a))
return A.d(a.slice(b,c),A.Q(a))},
b0(a,b){return this.X(a,b,null)},
ga0(a){if(a.length>0)return a[0]
throw A.f(A.iJ())},
K(a,b){var s,r
A.Q(a).h("m(1)").a(b)
s=a.length
for(r=0;r<s;++r){if(b.$1(a[r]))return!0
if(a.length!==s)throw A.f(A.Z(a))}return!1},
M(a,b){var s,r
A.Q(a).h("m(1)").a(b)
s=a.length
for(r=0;r<s;++r){if(!b.$1(a[r]))return!1
if(a.length!==s)throw A.f(A.Z(a))}return!0},
bD(a){var s,r,q,p,o,n
a.$flags&2&&A.r(a,"sort")
s=a.length
if(s<2)return
if(s===2){r=a[0]
q=a[1]
p=J.jM(r,q)
if(typeof p!=="number")return p.aY()
if(p>0){a[0]=q
a[1]=r}return}o=0
if(A.Q(a).c.b(null))for(n=0;n<a.length;++n)if(a[n]===void 0){a[n]=null;++o}a.sort(A.bO(J.mJ(),2))
if(o>0)this.bY(a,o)},
bY(a,b){var s,r=a.length
for(;s=r-1,r>0;r=s)if(a[s]===null){a[s]=void 0;--b
if(b===0)break}},
a8(a,b){var s,r=a.length
if(0>=r)return-1
for(s=0;s<r;++s){if(!(s<a.length))return A.a(a,s)
if(J.S(a[s],b))return s}return-1},
J(a,b){var s
for(s=0;s<a.length;++s)if(J.S(a[s],b))return!0
return!1},
j(a){return A.iK(a,"[","]")},
gt(a){return new J.bV(a,a.length,A.Q(a).h("bV<1>"))},
gD(a){return A.ds(a)},
gm(a){return a.length},
v(a,b){if(!(b>=0&&b<a.length))throw A.f(A.fS(a,b))
return a[b]},
q(a,b,c){A.Q(a).c.a(c)
a.$flags&2&&A.r(a)
if(!(b>=0&&b<a.length))throw A.f(A.fS(a,b))
a[b]=c},
bs(a,b){var s
A.Q(a).h("m(1)").a(b)
if(0>=a.length)return-1
for(s=0;s<a.length;++s)if(b.$1(a[s]))return s
return-1},
$iV:1,
$ih:1,
$il:1}
J.da.prototype={
cm(a){var s,r,q
if(!Array.isArray(a))return null
s=a.$flags|0
if((s&4)!==0)r="const, "
else if((s&2)!==0)r="unmodifiable, "
else r=(s&1)!==0?"fixed, ":""
q="Instance of '"+A.dt(a)+"'"
if(r==="")return q
return q+" ("+r+"length: "+a.length+")"}}
J.ed.prototype={}
J.bV.prototype={
gp(){var s=this.d
return s==null?this.$ti.c.a(s):s},
l(){var s,r=this,q=r.a,p=q.length
if(r.b!==p){q=A.o(q)
throw A.f(q)}s=r.c
if(s>=p){r.d=null
return!1}r.d=q[s]
r.c=s+1
return!0},
$iH:1}
J.bv.prototype={
a6(a,b){var s
A.kj(b)
if(a<b)return-1
else if(a>b)return 1
else if(a===b){if(a===0){s=this.gaR(b)
if(this.gaR(a)===s)return 0
if(this.gaR(a))return-1
return 1}return 0}else if(isNaN(a)){if(isNaN(b))return 0
return 1}else return-1},
gaR(a){return a===0?1/a<0:a<0},
ak(a){var s,r
if(a>=0){if(a<=2147483647)return a|0}else if(a>=-2147483648){s=a|0
return a===s?s:s-1}r=Math.floor(a)
if(isFinite(r))return r
throw A.f(A.be(""+a+".floor()"))},
cf(a){if(a>0){if(a!==1/0)return Math.round(a)}else if(a>-1/0)return 0-Math.round(0-a)
throw A.f(A.be(""+a+".round()"))},
V(a,b,c){if(B.a.a6(b,c)>0)throw A.f(A.fI(b))
if(this.a6(a,b)<0)return b
if(this.a6(a,c)>0)return c
return a},
k(a,b){var s,r,q,p,o
if(b<2||b>36)throw A.f(A.ar(b,2,36,"radix",null))
s=a.toString(b)
r=s.length
q=r-1
if(!(q>=0))return A.a(s,q)
if(s.charCodeAt(q)!==41)return s
p=/^([\da-z]+)(?:\.([\da-z]+))?\(e\+(\d+)\)$/.exec(s)
if(p==null)A.M(A.be("Unexpected toString result: "+s))
r=p.length
if(1>=r)return A.a(p,1)
s=p[1]
if(3>=r)return A.a(p,3)
o=+p[3]
r=p[2]
if(r!=null){s+=r
o-=r.length}return s+B.c.ap("0",o)},
j(a){if(a===0&&1/a<0)return"-0.0"
else return""+a},
gD(a){var s,r,q,p,o=a|0
if(a===o)return o&536870911
s=Math.abs(a)
r=Math.log(s)/0.6931471805599453|0
q=Math.pow(2,r)
p=s<1?s/q:q/s
return((p*9007199254740992|0)+(p*3542243181176521|0))*599197+r*1259&536870911},
A(a,b){var s=a%b
if(s===0)return 0
if(s>0)return s
return s+b},
bF(a,b){if((a|0)===a)if(b>=1||b<-1)return a/b|0
return this.bg(a,b)},
u(a,b){return(a|0)===a?a/b|0:this.bg(a,b)},
bg(a,b){var s=a/b
if(s>=-2147483648&&s<=2147483647)return s|0
if(s>0){if(s!==1/0)return Math.floor(s)}else if(s>-1/0)return Math.ceil(s)
throw A.f(A.be("Result of truncating division is "+A.k(s)+": "+A.k(a)+" ~/ "+b))},
ab(a,b){if(b<0)throw A.f(A.fI(b))
return b>31?0:a<<b>>>0},
be(a,b){return b>31?0:a<<b>>>0},
B(a,b){var s
if(a>0)s=this.a_(a,b)
else{s=b>31?31:b
s=a>>s>>>0}return s},
bf(a,b){if(0>b)throw A.f(A.fI(b))
return this.a_(a,b)},
a_(a,b){return b>31?0:a>>>b},
gE(a){return A.bm(t.o)},
$iaD:1,
$iw:1,
$ia7:1}
J.c3.prototype={
gE(a){return A.bm(t.S)},
$iy:1,
$ib:1}
J.dc.prototype={
gE(a){return A.bm(t.i)},
$iy:1}
J.aS.prototype={
bk(a,b){return new A.dO(b,a,0)},
ac(a,b,c){return a.substring(b,A.ch(b,c,a.length))},
b1(a,b){return this.ac(a,b,null)},
ap(a,b){var s,r
if(0>=b)return""
if(b===1||a.length===0)return a
if(b!==b>>>0)throw A.f(B.G)
for(s=a,r="";;){if((b&1)===1)r=s+r
b=b>>>1
if(b===0)break
s+=s}return r},
n(a,b,c){var s=b-a.length
if(s<=0)return a
return this.ap(c,s)+a},
a8(a,b){var s=a.indexOf(b,0)
return s},
cb(a,b){var s=a.length,r=b.length
if(s+r>s)s-=r
return a.lastIndexOf(b,s)},
J(a,b){return A.ou(a,b,0)},
a6(a,b){var s
A.B(b)
if(a===b)s=0
else s=a<b?-1:1
return s},
j(a){return a},
gD(a){var s,r,q
for(s=a.length,r=0,q=0;q<s;++q){r=r+a.charCodeAt(q)&536870911
r=r+((r&524287)<<10)&536870911
r^=r>>6}r=r+((r&67108863)<<3)&536870911
r^=r>>11
return r+((r&16383)<<15)&536870911},
gE(a){return A.bm(t.N)},
gm(a){return a.length},
$iV:1,
$iy:1,
$iaD:1,
$ieq:1,
$iP:1}
A.bB.prototype={
i(a,b){var s,r,q,p,o,n,m,l,k,j,i,h,g=this
t.L.a(b)
s=b.length
if(s===0)return
r=g.a+s
q=g.b
p=q.length
if(p<r){o=r*2
if(o<1024)o=1024
else{n=o-1
n|=B.a.B(n,1)
n|=n>>>2
n|=n>>>4
n|=n>>>8
o=((n|n>>>16)>>>0)+1}m=new Uint8Array(o)
B.h.R(m,0,p,q)
g.b=m
q=m}if(t.p.b(b))B.h.R(q,g.a,r,b)
else for(p=g.a,l=b.length,k=q.$flags|0,j=0;j<s;++j){i=p+j
if(!(j<l))return A.a(b,j)
h=b[j]
k&2&&A.r(q)
if(!(i<q.length))return A.a(q,i)
q[i]=h}g.a=r},
bx(){var s,r=this
if(r.a===0)return $.cY()
s=J.iG(B.h.gaI(r.b),r.b.byteOffset,r.a)
r.a=0
r.b=$.cY()
return s},
cl(){var s=this
if(s.a===0)return $.cY()
return new Uint8Array(A.z(J.iG(B.h.gaI(s.b),s.b.byteOffset,s.a)))},
gm(a){return this.a},
$id2:1}
A.de.prototype={
j(a){return"LateInitializationError: "+this.a}}
A.et.prototype={}
A.bY.prototype={}
A.O.prototype={
gt(a){var s=this
return new A.ad(s,s.gm(s),A.A(s).h("ad<O.E>"))},
M(a,b){var s,r,q=this
A.A(q).h("m(O.E)").a(b)
s=q.gm(q)
for(r=0;r<s;++r){if(!b.$1(q.I(0,r)))return!1
if(s!==q.gm(q))throw A.f(A.Z(q))}return!0},
K(a,b){var s,r,q=this
A.A(q).h("m(O.E)").a(b)
s=q.gm(q)
for(r=0;r<s;++r){if(b.$1(q.I(0,r)))return!0
if(s!==q.gm(q))throw A.f(A.Z(q))}return!1},
H(a,b){var s,r,q,p=this,o=p.gm(p)
if(b.length!==0){if(o===0)return""
s=A.k(p.I(0,0))
if(o!==p.gm(p))throw A.f(A.Z(p))
for(r=s,q=1;q<o;++q){r=r+b+A.k(p.I(0,q))
if(o!==p.gm(p))throw A.f(A.Z(p))}return r.charCodeAt(0)==0?r:r}else{for(q=0,r="";q<o;++q){r+=A.k(p.I(0,q))
if(o!==p.gm(p))throw A.f(A.Z(p))}return r.charCodeAt(0)==0?r:r}}}
A.cm.prototype={
gbO(){var s=J.bt(this.a),r=this.c
if(r==null||r>s)return s
return r},
gc0(){var s=J.bt(this.a),r=this.b
if(r>s)return s
return r},
gm(a){var s,r=J.bt(this.a),q=this.b
if(q>=r)return 0
s=this.c
if(s==null||s>=r)return r-q
return s-q},
I(a,b){var s=this,r=s.gc0()+b
if(b<0||r>=s.gbO())throw A.f(A.ec(b,s.gm(0),s,null,"index"))
return J.iH(s.a,r)}}
A.ad.prototype={
gp(){var s=this.d
return s==null?this.$ti.c.a(s):s},
l(){var s,r=this,q=r.a,p=J.kO(q),o=p.gm(q)
if(r.b!==o)throw A.f(A.Z(q))
s=r.c
if(s>=o){r.d=null
return!1}r.d=p.I(q,s);++r.c
return!0},
$iH:1}
A.cb.prototype={
gt(a){var s=this.a
return new A.cc(s.gt(s),this.b,A.A(this).h("cc<1,2>"))},
gm(a){var s=this.a
return s.gm(s)}}
A.bZ.prototype={}
A.cc.prototype={
l(){var s=this,r=s.b
if(r.l()){s.a=s.c.$1(r.gp())
return!0}s.a=null
return!1},
gp(){var s=this.a
return s==null?this.$ti.y[1].a(s):s},
$iH:1}
A.ae.prototype={
gm(a){return J.bt(this.a)},
I(a,b){return this.b.$1(J.iH(this.a,b))}}
A.aL.prototype={
gt(a){return new A.bf(J.bU(this.a),this.b,this.$ti.h("bf<1>"))}}
A.bf.prototype={
l(){var s,r
for(s=this.a,r=this.b;s.l();)if(r.$1(s.gp()))return!0
return!1},
gp(){return this.a.gp()},
$iH:1}
A.c_.prototype={
gt(a){return B.z},
gm(a){return 0},
M(a,b){this.$ti.h("m(1)").a(b)
return!0}}
A.c0.prototype={
l(){return!1},
gp(){throw A.f(A.iJ())},
$iH:1}
A.bA.prototype={
gt(a){return new A.co(J.bU(this.a),this.$ti.h("co<1>"))}}
A.co.prototype={
l(){var s,r
for(s=this.a,r=this.$ti.c;s.l();)if(r.b(s.gp()))return!0
return!1},
gp(){return this.$ti.c.a(this.a.gp())},
$iH:1}
A.G.prototype={
sm(a,b){throw A.f(A.be("Cannot change the length of a fixed-length list"))},
i(a,b){A.a6(a).h("G.E").a(b)
throw A.f(A.be("Cannot add to a fixed-length list"))}}
A.aH.prototype={
gm(a){return this.a.length},
I(a,b){var s=this.a
return J.iH(s,s.length-1-b)}}
A.i.prototype={$r:"+(1,2)",$s:1}
A.cw.prototype={$r:"+boxes,pixels(1,2)",$s:2}
A.bE.prototype={$r:"+(1,2,3)",$s:3}
A.cx.prototype={$r:"+boxes,from,index,pixels(1,2,3,4)",$s:4}
A.cy.prototype={$r:"+col,fineX,fineY,key,row(1,2,3,4,5)",$s:5}
A.cr.prototype={
gp(){var s=this.d
return s==null?this.$ti.c.a(s):s},
l(){var s=this,r=s.c
if(r>=s.b){s.d=null
return!1}s.d=s.a[r]
s.c=r+1
return!0},
$iH:1}
A.bW.prototype={
i(a,b){A.A(this).c.a(b)
A.lz()}}
A.bX.prototype={
gm(a){return this.b},
gt(a){var s,r=this,q=r.$keys
if(q==null){q=Object.keys(r.a)
r.$keys=q}s=q
return new A.cr(s,s.length,r.$ti.h("cr<1>"))}}
A.cj.prototype={}
A.eI.prototype={
L(a){var s,r,q=this,p=new RegExp(q.a).exec(a)
if(p==null)return null
s=Object.create(null)
r=q.b
if(r!==-1)s.arguments=p[r+1]
r=q.c
if(r!==-1)s.argumentsExpr=p[r+1]
r=q.d
if(r!==-1)s.expr=p[r+1]
r=q.e
if(r!==-1)s.method=p[r+1]
r=q.f
if(r!==-1)s.receiver=p[r+1]
return s}}
A.cf.prototype={
j(a){return"Null check operator used on a null value"}}
A.dd.prototype={
j(a){var s,r=this,q="NoSuchMethodError: method not found: '",p=r.b
if(p==null)return"NoSuchMethodError: "+r.a
s=r.c
if(s==null)return q+p+"' ("+r.a+")"
return q+p+"' on '"+s+"' ("+r.a+")"}}
A.dC.prototype={
j(a){var s=this.a
return s.length===0?"Error":"Error: "+s}}
A.eo.prototype={
j(a){return"Throw of null ('"+(this.a===null?"null":"undefined")+"' from JavaScript)"}}
A.c1.prototype={}
A.cA.prototype={
j(a){var s,r=this.b
if(r!=null)return r
r=this.a
s=r!==null&&typeof r==="object"?r.stack:null
return this.b=s==null?"":s},
$iaX:1}
A.aR.prototype={
j(a){var s=this.constructor,r=s==null?null:s.name
return"Closure '"+A.l8(r==null?"unknown":r)+"'"},
$ib4:1,
gco(){return this},
$C:"$1",
$R:1,
$D:null}
A.d3.prototype={$C:"$0",$R:0}
A.d4.prototype={$C:"$2",$R:2}
A.dz.prototype={}
A.dw.prototype={
j(a){var s=this.$static_name
if(s==null)return"Closure of unknown static method"
return"Closure '"+A.l8(s)+"'"}}
A.bu.prototype={
N(a,b){if(b==null)return!1
if(this===b)return!0
if(!(b instanceof A.bu))return!1
return this.$_target===b.$_target&&this.a===b.a},
gD(a){return(A.kU(this.a)^A.ds(this.$_target))>>>0},
j(a){return"Closure '"+this.$_name+"' of "+("Instance of '"+A.dt(this.a)+"'")}}
A.du.prototype={
j(a){return"RuntimeError: "+this.a}}
A.b6.prototype={
gm(a){return this.a},
aj(a){var s,r
if(typeof a=="string"){s=this.b
if(s==null)return!1
return s[a]!=null}else if(typeof a=="number"&&(a&0x3fffffff)===a){r=this.c
if(r==null)return!1
return r[a]!=null}else return this.c9(a)},
c9(a){var s=this.d
if(s==null)return!1
return this.aP(s[this.aO(a)],a)>=0},
v(a,b){var s,r,q,p,o=null
if(typeof b=="string"){s=this.b
if(s==null)return o
r=s[b]
q=r==null?o:r.b
return q}else if(typeof b=="number"&&(b&0x3fffffff)===b){p=this.c
if(p==null)return o
r=p[b]
q=r==null?o:r.b
return q}else return this.ca(b)},
ca(a){var s,r,q=this.d
if(q==null)return null
s=q[this.aO(a)]
r=this.aP(s,a)
if(r<0)return null
return s[r].b},
q(a,b,c){var s,r,q,p,o,n,m=this,l=A.A(m)
l.c.a(b)
l.y[1].a(c)
if(typeof b=="string"){s=m.b
m.b3(s==null?m.b=m.aE():s,b,c)}else if(typeof b=="number"&&(b&0x3fffffff)===b){r=m.c
m.b3(r==null?m.c=m.aE():r,b,c)}else{q=m.d
if(q==null)q=m.d=m.aE()
p=m.aO(b)
o=q[p]
if(o==null)q[p]=[m.ar(b,c)]
else{n=m.aP(o,b)
if(n>=0)o[n].b=c
else o.push(m.ar(b,c))}}},
a2(a,b){var s,r,q=this,p=A.A(q)
p.c.a(a)
p.h("2()").a(b)
if(q.aj(a)){s=q.v(0,a)
return s==null?p.y[1].a(s):s}r=b.$0()
q.q(0,a,r)
return r},
c7(a,b){var s,r,q=this
A.A(q).h("~(1,2)").a(b)
s=q.e
r=q.r
while(s!=null){b.$2(s.a,s.b)
if(r!==q.r)throw A.f(A.Z(q))
s=s.c}},
b3(a,b,c){var s,r=A.A(this)
r.c.a(b)
r.y[1].a(c)
s=a[b]
if(s==null)a[b]=this.ar(b,c)
else s.b=c},
ar(a,b){var s=this,r=A.A(s),q=new A.ef(r.c.a(a),r.y[1].a(b))
if(s.e==null)s.e=s.f=q
else s.f=s.f.c=q;++s.a
s.r=s.r+1&1073741823
return q},
aO(a){return J.aw(a)&1073741823},
aP(a,b){var s,r
if(a==null)return-1
s=a.length
for(r=0;r<s;++r)if(J.S(a[r].a,b))return r
return-1},
j(a){return A.jT(this)},
aE(){var s=Object.create(null)
s["<non-identifier-key>"]=s
delete s["<non-identifier-key>"]
return s},
$ijP:1}
A.ef.prototype={}
A.c7.prototype={
gm(a){return this.a.a},
gt(a){var s=this.a
return new A.b8(s,s.r,s.e,this.$ti.h("b8<1>"))}}
A.b8.prototype={
gp(){return this.d},
l(){var s,r=this,q=r.a
if(r.b!==q.r)throw A.f(A.Z(q))
s=r.c
if(s==null){r.d=null
return!1}else{r.d=s.a
r.c=s.c
return!0}},
$iH:1}
A.c8.prototype={
gm(a){return this.a.a},
gt(a){var s=this.a
return new A.aG(s,s.r,s.e,this.$ti.h("aG<1>"))}}
A.aG.prototype={
gp(){return this.d},
l(){var s,r=this,q=r.a
if(r.b!==q.r)throw A.f(A.Z(q))
s=r.c
if(s==null){r.d=null
return!1}else{r.d=s.b
r.c=s.c
return!0}},
$iH:1}
A.N.prototype={
gm(a){return this.a.a},
gt(a){var s=this.a
return new A.b7(s,s.r,s.e,this.$ti.h("b7<1,2>"))}}
A.b7.prototype={
gp(){var s=this.d
s.toString
return s},
l(){var s,r=this,q=r.a
if(r.b!==q.r)throw A.f(A.Z(q))
s=r.c
if(s==null){r.d=null
return!1}else{r.d=new A.W(s.a,s.b,r.$ti.h("W<1,2>"))
r.c=s.c
return!0}},
$iH:1}
A.fY.prototype={
$1(a){return this.a(a)},
$S:43}
A.fZ.prototype={
$2(a,b){return this.a(a,b)},
$S:31}
A.h_.prototype={
$1(a){return this.a(A.B(a))},
$S:19}
A.ag.prototype={
j(a){return this.bi(!1)},
bi(a){var s,r,q,p,o,n=this.bR(),m=this.af(),l=(a?"Record ":"")+"("
for(s=n.length,r="",q=0;q<s;++q,r=", "){l+=r
p=n[q]
if(typeof p=="string")l=l+p+": "
if(!(q<m.length))return A.a(m,q)
o=m[q]
l=a?l+A.jX(o):l+A.k(o)}l+=")"
return l.charCodeAt(0)==0?l:l},
bR(){var s,r=this.$s
while($.f2.length<=r)B.b.i($.f2,null)
s=$.f2[r]
if(s==null){s=this.bM()
B.b.q($.f2,r,s)}return s},
bM(){var s,r,q,p=this.$r,o=p.indexOf("("),n=p.substring(1,o),m=p.substring(o),l=m==="()"?0:m.replace(/[^,]/g,"").length+1,k=t.K,j=J.iM(l,k)
for(s=0;s<l;++s)j[s]=s
if(n!==""){r=n.split(",")
s=r.length
for(q=l;s>0;){--q;--s
B.b.q(j,q,r[s])}}return A.eh(j,k)}}
A.bj.prototype={
af(){return[this.a,this.b]},
N(a,b){if(b==null)return!1
return b instanceof A.bj&&this.$s===b.$s&&J.S(this.a,b.a)&&J.S(this.b,b.b)},
gD(a){return A.iQ(this.$s,this.a,this.b,B.l)}}
A.bD.prototype={
af(){return[this.a,this.b,this.c]},
N(a,b){var s=this
if(b==null)return!1
return b instanceof A.bD&&s.$s===b.$s&&J.S(s.a,b.a)&&J.S(s.b,b.b)&&J.S(s.c,b.c)},
gD(a){var s=this
return A.iQ(s.$s,s.a,s.b,s.c)}}
A.bk.prototype={
af(){return this.a},
N(a,b){if(b==null)return!1
return b instanceof A.bk&&this.$s===b.$s&&A.me(this.a,b.a)},
gD(a){return A.iQ(this.$s,A.lQ(this.a),B.l,B.l)}}
A.c5.prototype={
j(a){return"RegExp/"+this.a+"/"+this.b.flags},
gba(){var s=this,r=s.c
if(r!=null)return r
r=s.b
return s.c=A.jN(s.a,r.multiline,!r.ignoreCase,r.unicode,r.dotAll,"g")},
bk(a,b){return new A.dD(this,b,0)},
bQ(a,b){var s,r=this.gba()
if(r==null)r=A.bF(r)
r.lastIndex=b
s=r.exec(a)
if(s==null)return null
return new A.dL(s)},
$ieq:1,
$ilS:1}
A.dL.prototype={
gb_(){return this.b.index},
gaN(){var s=this.b
return s.index+s[0].length},
$iby:1,
$ici:1}
A.dD.prototype={
gt(a){return new A.dE(this.a,this.b,this.c)}}
A.dE.prototype={
gp(){var s=this.d
return s==null?t.a0.a(s):s},
l(){var s,r,q,p,o,n,m=this,l=m.b
if(l==null)return!1
s=m.c
r=l.length
if(s<=r){q=m.a
p=q.bQ(l,s)
if(p!=null){m.d=p
o=p.gaN()
if(p.b.index===o){s=!1
if(q.b.unicode){q=m.c
n=q+1
if(n<r){if(!(q>=0&&q<r))return A.a(l,q)
q=l.charCodeAt(q)
if(q>=55296&&q<=56319){if(!(n>=0))return A.a(l,n)
s=l.charCodeAt(n)
s=s>=56320&&s<=57343}}}o=(s?o+1:o)+1}m.c=o
return!0}}m.b=m.d=null
return!1},
$iH:1}
A.dy.prototype={
gaN(){return this.a+this.c.length},
$iby:1,
gb_(){return this.a}}
A.dO.prototype={
gt(a){return new A.dP(this.a,this.b,this.c)}}
A.dP.prototype={
l(){var s,r,q=this,p=q.c,o=q.b,n=o.length,m=q.a,l=m.length
if(p+n>l){q.d=null
return!1}s=m.indexOf(o,p)
if(s<0){q.c=l+1
q.d=null
return!1}r=s+n
q.d=new A.dy(s,o)
q.c=r===q.c?r+1:r
return!0},
gp(){var s=this.d
s.toString
return s},
$iH:1}
A.aV.prototype={
gE(a){return B.aC},
bo(a,b,c){return c==null?new Uint8Array(a,b):new Uint8Array(a,b,c)},
$iy:1,
$iaV:1}
A.bz.prototype={$ibz:1}
A.ce.prototype={
gaI(a){if(((a.$flags|0)&2)!==0)return new A.f8(a.buffer)
else return a.buffer},
bT(a,b,c,d){var s=A.ar(b,0,c,d,null)
throw A.f(s)},
b7(a,b,c,d){if(b>>>0!==b||b>c)this.bT(a,b,c,d)}}
A.f8.prototype={
bo(a,b,c){var s=A.jU(this.a,b,c)
s.$flags=3
return s}}
A.dh.prototype={
gE(a){return B.aD},
$iy:1}
A.X.prototype={
gm(a){return a.length},
c_(a,b,c,d,e){var s,r,q=a.length
this.b7(a,b,q,"start")
this.b7(a,c,q,"end")
if(b>c)throw A.f(A.ar(b,0,c,null,null))
s=c-b
r=d.length
if(r-e<s)throw A.f(A.eG("Not enough elements"))
if(e!==0||r!==s)d=d.subarray(e,e+s)
a.set(d,b)},
$iV:1,
$iab:1}
A.cd.prototype={
R(a,b,c,d){t.k.a(d)
a.$flags&2&&A.r(a,5)
this.b2(a,b,c,d,0)},
$ih:1,
$il:1}
A.af.prototype={
q(a,b,c){A.j(c)
a.$flags&2&&A.r(a)
A.mv(b,a,a.length)
a[b]=c},
R(a,b,c,d){t.D.a(d)
a.$flags&2&&A.r(a,5)
if(t.E.b(d)){this.c_(a,b,c,d,0)
return}this.b2(a,b,c,d,0)},
$ih:1,
$il:1}
A.di.prototype={
gE(a){return B.aE},
$iy:1}
A.dj.prototype={
gE(a){return B.aF},
$iy:1}
A.dk.prototype={
gE(a){return B.aG},
$iy:1}
A.dl.prototype={
gE(a){return B.aH},
$iy:1}
A.dm.prototype={
gE(a){return B.aI},
$iy:1}
A.dn.prototype={
gE(a){return B.aK},
$iy:1}
A.dp.prototype={
gE(a){return B.aL},
$iy:1}
A.b9.prototype={
gE(a){return B.aM},
gm(a){return a.length},
$iy:1,
$ib9:1}
A.ba.prototype={
gE(a){return B.aN},
gm(a){return a.length},
X(a,b,c){return new Uint8Array(a.subarray(b,A.km(b,c,a.length)))},
$iy:1,
$iba:1,
$ial:1}
A.cs.prototype={}
A.ct.prototype={}
A.cu.prototype={}
A.cv.prototype={}
A.as.prototype={
h(a){return A.cF(v.typeUniverse,this,a)},
F(a){return A.kg(v.typeUniverse,this,a)}}
A.dJ.prototype={}
A.f6.prototype={
j(a){return A.ah(this.a,null)}}
A.dI.prototype={
j(a){return this.a}}
A.cB.prototype={$iaJ:1}
A.eM.prototype={
$1(a){var s=this.a,r=s.a
s.a=null
r.$0()},
$S:12}
A.eL.prototype={
$1(a){var s,r
this.a.a=t.M.a(a)
s=this.b
r=this.c
s.firstChild?s.removeChild(r):s.appendChild(r)},
$S:37}
A.eN.prototype={
$0(){this.a.$0()},
$S:13}
A.eO.prototype={
$0(){this.a.$0()},
$S:13}
A.f4.prototype={
bH(a,b){if(self.setTimeout!=null)self.setTimeout(A.bO(new A.f5(this,b),0),a)
else throw A.f(A.be("`setTimeout()` not found."))}}
A.f5.prototype={
$0(){this.b.$0()},
$S:0}
A.dF.prototype={
aJ(a){var s,r=this,q=r.$ti
q.h("1/?").a(a)
if(a==null)a=q.c.a(a)
if(!r.b)r.a.b5(a)
else{s=r.a
if(q.h("b5<1>").b(a))s.b6(a)
else s.b8(a)}},
aK(a,b){var s=this.a
if(this.b)s.aw(new A.ak(a,b))
else s.av(new A.ak(a,b))}}
A.fc.prototype={
$1(a){return this.a.$2(0,a)},
$S:8}
A.fd.prototype={
$2(a,b){this.a.$2(1,new A.c1(a,t.l.a(b)))},
$S:49}
A.fG.prototype={
$2(a,b){this.a(A.j(a),b)},
$S:22}
A.ak.prototype={
j(a){return A.k(this.a)},
$iD:1,
ga4(){return this.b}}
A.dH.prototype={
aK(a,b){var s=this.a
if((s.a&30)!==0)throw A.f(A.eG("Future already completed"))
s.av(A.mI(a,b))},
bq(a){return this.aK(a,null)}}
A.cp.prototype={
aJ(a){var s,r=this.$ti
r.h("1/?").a(a)
s=this.a
if((s.a&30)!==0)throw A.f(A.eG("Future already completed"))
s.b5(r.h("1/").a(a))}}
A.bg.prototype={
cc(a){if((this.c&15)!==6)return!0
return this.b.b.aW(t.bG.a(this.d),a.a,t.y,t.K)},
c8(a){var s,r=this,q=r.e,p=null,o=t.z,n=t.K,m=a.a,l=r.b.b
if(t.V.b(q))p=l.ci(q,m,a.b,o,n,t.l)
else p=l.aW(t.v.a(q),m,o,n)
try{o=r.$ti.h("2/").a(p)
return o}catch(s){if(t.b7.b(A.aQ(s))){if((r.c&1)!==0)throw A.f(A.C("The error handler of Future.then must return a value of the returned future's type","onError"))
throw A.f(A.C("The error handler of Future.catchError must return a value of the future's type","onError"))}else throw s}}}
A.T.prototype={
by(a,b,c){var s,r,q=this.$ti
q.F(c).h("1/(2)").a(a)
s=$.K
if(s===B.i){if(!t.V.b(b)&&!t.v.b(b))throw A.f(A.jG(b,"onError",u.c))}else{c.h("@<0/>").F(q.c).h("1(2)").a(a)
b=A.n3(b,s)}r=new A.T(s,c.h("T<0>"))
this.au(new A.bg(r,3,a,b,q.h("@<1>").F(c).h("bg<1,2>")))
return r},
bh(a,b,c){var s,r=this.$ti
r.F(c).h("1/(2)").a(a)
s=new A.T($.K,c.h("T<0>"))
this.au(new A.bg(s,19,a,b,r.h("@<1>").F(c).h("bg<1,2>")))
return s},
bZ(a){this.a=this.a&1|16
this.c=a},
ae(a){this.a=a.a&30|this.a&1
this.c=a.c},
au(a){var s,r=this,q=r.a
if(q<=3){a.a=t.F.a(r.c)
r.c=a}else{if((q&4)!==0){s=t._.a(r.c)
if((s.a&24)===0){s.au(a)
return}r.ae(s)}A.dR(null,null,r.b,t.M.a(new A.eS(r,a)))}},
bb(a){var s,r,q,p,o,n,m=this,l={}
l.a=a
if(a==null)return
s=m.a
if(s<=3){r=t.F.a(m.c)
m.c=a
if(r!=null){q=a.a
for(p=a;q!=null;p=q,q=o)o=q.a
p.a=r}}else{if((s&4)!==0){n=t._.a(m.c)
if((n.a&24)===0){n.bb(a)
return}m.ae(n)}l.a=m.ah(a)
A.dR(null,null,m.b,t.M.a(new A.eW(l,m)))}},
ag(){var s=t.F.a(this.c)
this.c=null
return this.ah(s)},
ah(a){var s,r,q
for(s=a,r=null;s!=null;r=s,s=q){q=s.a
s.a=r}return r},
b8(a){var s,r=this
r.$ti.c.a(a)
s=r.ag()
r.a=8
r.c=a
A.bC(r,s)},
bL(a){var s,r,q=this
if((a.a&16)!==0){s=q.b===a.b
s=!(s||s)}else s=!1
if(s)return
r=q.ag()
q.ae(a)
A.bC(q,r)},
aw(a){var s=this.ag()
this.bZ(a)
A.bC(this,s)},
b5(a){var s=this.$ti
s.h("1/").a(a)
if(s.h("b5<1>").b(a)){this.b6(a)
return}this.bK(a)},
bK(a){var s=this
s.$ti.c.a(a)
s.a^=2
A.dR(null,null,s.b,t.M.a(new A.eU(s,a)))},
b6(a){A.iT(this.$ti.h("b5<1>").a(a),this,!1)
return},
av(a){this.a^=2
A.dR(null,null,this.b,t.M.a(new A.eT(this,a)))},
$ib5:1}
A.eS.prototype={
$0(){A.bC(this.a,this.b)},
$S:0}
A.eW.prototype={
$0(){A.bC(this.b,this.a.a)},
$S:0}
A.eV.prototype={
$0(){A.iT(this.a.a,this.b,!0)},
$S:0}
A.eU.prototype={
$0(){this.a.b8(this.b)},
$S:0}
A.eT.prototype={
$0(){this.a.aw(this.b)},
$S:0}
A.eZ.prototype={
$0(){var s,r,q,p,o,n,m,l,k=this,j=null
try{q=k.a.a
j=q.b.b.cg(t.bd.a(q.d),t.z)}catch(p){s=A.aQ(p)
r=A.bR(p)
if(k.c&&t.u.a(k.b.a.c).a===s){q=k.a
q.c=t.u.a(k.b.a.c)}else{q=s
o=r
if(o==null)o=A.iI(q)
n=k.a
n.c=new A.ak(q,o)
q=n}q.b=!0
return}if(j instanceof A.T&&(j.a&24)!==0){if((j.a&16)!==0){q=k.a
q.c=t.u.a(j.c)
q.b=!0}return}if(j instanceof A.T){m=k.b.a
l=new A.T(m.b,m.$ti)
j.by(new A.f_(l,m),new A.f0(l),t.H)
q=k.a
q.c=l
q.b=!1}},
$S:0}
A.f_.prototype={
$1(a){this.a.bL(this.b)},
$S:12}
A.f0.prototype={
$2(a,b){A.bF(a)
t.l.a(b)
this.a.aw(new A.ak(a,b))},
$S:23}
A.eY.prototype={
$0(){var s,r,q,p,o,n,m,l
try{q=this.a
p=q.a
o=p.$ti
n=o.c
m=n.a(this.b)
q.c=p.b.b.aW(o.h("2/(1)").a(p.d),m,o.h("2/"),n)}catch(l){s=A.aQ(l)
r=A.bR(l)
q=s
p=r
if(p==null)p=A.iI(q)
o=this.a
o.c=new A.ak(q,p)
o.b=!0}},
$S:0}
A.eX.prototype={
$0(){var s,r,q,p,o,n,m,l=this
try{s=t.u.a(l.a.a.c)
p=l.b
if(p.a.cc(s)&&p.a.e!=null){p.c=p.a.c8(s)
p.b=!1}}catch(o){r=A.aQ(o)
q=A.bR(o)
p=t.u.a(l.a.a.c)
if(p.a===r){n=l.b
n.c=p
p=n}else{p=r
n=q
if(n==null)n=A.iI(p)
m=l.b
m.c=new A.ak(p,n)
p=m}p.b=!0}},
$S:0}
A.dG.prototype={}
A.dN.prototype={}
A.cG.prototype={$ik3:1}
A.dM.prototype={
cj(a){var s,r,q
t.M.a(a)
try{if(B.i===$.K){a.$0()
return}A.kx(null,null,this,a,t.H)}catch(q){s=A.aQ(q)
r=A.bR(q)
A.j8(A.bF(s),t.l.a(r))}},
c5(a){return new A.f3(this,t.M.a(a))},
cg(a,b){b.h("0()").a(a)
if($.K===B.i)return a.$0()
return A.kx(null,null,this,a,b)},
aW(a,b,c,d){c.h("@<0>").F(d).h("1(2)").a(a)
d.a(b)
if($.K===B.i)return a.$1(b)
return A.n5(null,null,this,a,b,c,d)},
ci(a,b,c,d,e,f){d.h("@<0>").F(e).F(f).h("1(2,3)").a(a)
e.a(b)
f.a(c)
if($.K===B.i)return a.$2(b,c)
return A.n4(null,null,this,a,b,c,d,e,f)},
bv(a,b,c,d){return b.h("@<0>").F(c).F(d).h("1(2,3)").a(a)}}
A.f3.prototype={
$0(){return this.a.cj(this.b)},
$S:0}
A.fA.prototype={
$0(){A.lB(this.a,this.b)},
$S:0}
A.aN.prototype={
gt(a){var s=this,r=new A.aO(s,s.r,A.A(s).h("aO<1>"))
r.c=s.e
return r},
gm(a){return this.a},
J(a,b){var s
if(typeof b=="number"&&(b&1073741823)===b){s=this.c
if(s==null)return!1
return t.O.a(s[b])!=null}else return this.bN(b)},
bN(a){var s=this.d
if(s==null)return!1
return this.aC(s[this.az(a)],a)>=0},
i(a,b){var s,r,q=this
A.A(q).c.a(b)
if(typeof b=="string"&&b!=="__proto__"){s=q.b
return q.b4(s==null?q.b=A.iU():s,b)}else if(typeof b=="number"&&(b&1073741823)===b){r=q.c
return q.b4(r==null?q.c=A.iU():r,b)}else return q.bI(b)},
bI(a){var s,r,q,p=this
A.A(p).c.a(a)
s=p.d
if(s==null)s=p.d=A.iU()
r=p.az(a)
q=s[r]
if(q==null)s[r]=[p.aF(a)]
else{if(p.aC(q,a)>=0)return!1
q.push(p.aF(a))}return!0},
aV(a,b){var s=this
if(typeof b=="string"&&b!=="__proto__")return s.bd(s.b,b)
else if(typeof b=="number"&&(b&1073741823)===b)return s.bd(s.c,b)
else return s.bX(b)},
bX(a){var s,r,q,p,o=this,n=o.d
if(n==null)return!1
s=o.az(a)
r=n[s]
q=o.aC(r,a)
if(q<0)return!1
p=r.splice(q,1)[0]
if(0===r.length)delete n[s]
o.bj(p)
return!0},
P(a){var s=this
if(s.a>0){s.b=s.c=s.d=s.e=s.f=null
s.a=0
s.aD()}},
b4(a,b){A.A(this).c.a(b)
if(t.O.a(a[b])!=null)return!1
a[b]=this.aF(b)
return!0},
bd(a,b){var s
if(a==null)return!1
s=t.O.a(a[b])
if(s==null)return!1
this.bj(s)
delete a[b]
return!0},
aD(){this.r=this.r+1&1073741823},
aF(a){var s,r=this,q=new A.dK(A.A(r).c.a(a))
if(r.e==null)r.e=r.f=q
else{s=r.f
s.toString
q.c=s
r.f=s.b=q}++r.a
r.aD()
return q},
bj(a){var s=this,r=a.c,q=a.b
if(r==null)s.e=q
else r.b=q
if(q==null)s.f=r
else q.c=r;--s.a
s.aD()},
az(a){return J.aw(a)&1073741823},
aC(a,b){var s,r
if(a==null)return-1
s=a.length
for(r=0;r<s;++r)if(J.S(a[r].a,b))return r
return-1},
$ijQ:1}
A.dK.prototype={}
A.aO.prototype={
gp(){var s=this.d
return s==null?this.$ti.c.a(s):s},
l(){var s=this,r=s.c,q=s.a
if(s.b!==q.r)throw A.f(A.Z(q))
else if(r==null){s.d=null
return!1}else{s.d=s.$ti.h("1?").a(r.a)
s.c=r.b
return!0}},
$iH:1}
A.v.prototype={
gt(a){return new A.ad(a,a.length,A.a6(a).h("ad<v.E>"))},
I(a,b){if(!(b>=0&&b<a.length))return A.a(a,b)
return a[b]},
M(a,b){var s,r,q
A.a6(a).h("m(v.E)").a(b)
s=a.length
for(r=s,q=0;q<s;++q){if(!(q<r))return A.a(a,q)
if(!b.$1(a[q]))return!1
r=a.length
if(s!==r)throw A.f(A.Z(a))}return!0},
K(a,b){var s,r,q
A.a6(a).h("m(v.E)").a(b)
s=a.length
for(r=s,q=0;q<s;++q){if(!(q<r))return A.a(a,q)
if(b.$1(a[q]))return!0
r=a.length
if(s!==r)throw A.f(A.Z(a))}return!1},
H(a,b){var s
if(a.length===0)return""
s=A.iS("",a,b)
return s.charCodeAt(0)==0?s:s},
W(a){return this.H(a,"")},
bt(a,b,c){var s=A.a6(a)
return new A.ae(a,s.F(c).h("1(v.E)").a(b),s.h("@<v.E>").F(c).h("ae<1,2>"))},
i(a,b){var s
A.a6(a).h("v.E").a(b)
s=a.length
this.sm(a,s+1)
if(!(s<a.length))return A.a(a,s)
a[s]=b},
a7(a,b,c,d){var s,r
A.a6(a).h("v.E?").a(d)
s=a.length
A.ch(b,c,s)
for(r=b;r<c;++r){if(!(r<s))return A.a(a,r)
a[r]=d}},
aZ(a,b,c,d,e){var s,r,q,p,o,n
A.a6(a).h("h<v.E>").a(d)
s=a.length
A.ch(b,c,s)
r=c-b
if(r===0)return
A.es(e,"skipCount")
q=d.length
if(e+r>q)throw A.f(A.eG("Too few elements"))
if(e<b)for(p=r-1;p>=0;--p){o=b+p
n=e+p
if(!(n<q))return A.a(d,n)
n=d[n]
if(!(o<s))return A.a(a,o)
a[o]=n}else for(p=0;p<r;++p){o=b+p
n=e+p
if(!(n<q))return A.a(d,n)
n=d[n]
if(!(o>=0&&o<s))return A.a(a,o)
a[o]=n}},
R(a,b,c,d){return this.aZ(a,b,c,d,0)},
bB(a,b,c){A.a6(a).h("h<v.E>").a(c)
this.R(a,b,b+c.length,c)},
j(a){return A.iK(a,"[","]")}}
A.ca.prototype={
cn(a,b,c){var s,r=this,q=A.A(r)
q.c.a(a)
q.h("2(2)").a(b)
q.h("2()?").a(c)
if(r.aj(a)){s=r.v(0,a)
q=b.$1(s==null?q.y[1].a(s):s)
r.q(0,a,q)
return q}q=c.$0()
r.q(0,a,q)
return q},
gm(a){return this.a},
j(a){return A.jT(this)},
$idg:1}
A.ek.prototype={
$2(a,b){var s,r=this.a
if(!r.a)this.b.a+=", "
r.a=!1
r=this.b
s=A.k(a)
r.a=(r.a+=s)+": "
s=A.k(b)
r.a+=s},
$S:27}
A.aW.prototype={
C(a,b){var s
A.A(this).h("h<1>").a(b)
for(s=b.gt(b);s.l();)this.i(0,s.gp())},
j(a){return A.iK(this,"{","}")},
$ih:1,
$iay:1}
A.cz.prototype={}
A.ea.prototype={
aL(a){var s,r,q,p=A.ch(0,null,a.length)
if(0===p)return new Uint8Array(0)
s=new A.eP()
r=s.c6(a,0,p)
r.toString
q=s.a
if(q<-1)A.M(A.aE("Missing padding character",a,p))
if(q>0)A.M(A.aE("Invalid length, must be multiple of four",a,p))
s.a=-1
return r}}
A.eP.prototype={
c6(a,b,c){var s,r=this,q=r.a
if(q<0){r.a=A.k4(a,b,c,q)
return null}if(b===c)return new Uint8Array(0)
s=A.m4(a,b,c,q)
r.a=A.m6(a,b,c,s,0,r.a)
return s}}
A.d6.prototype={}
A.eK.prototype={
aL(a){var s,r,q,p=a.length,o=A.ch(0,null,p)
if(o===0)return new Uint8Array(0)
s=new Uint8Array(o*3)
r=new A.f9(s)
if(r.bS(a,0,o)!==o){q=o-1
if(!(q>=0&&q<p))return A.a(a,q)
r.aG()}return B.h.X(s,0,r.b)}}
A.f9.prototype={
aG(){var s,r=this,q=r.c,p=r.b,o=r.b=p+1
q.$flags&2&&A.r(q)
s=q.length
if(!(p<s))return A.a(q,p)
q[p]=239
p=r.b=o+1
if(!(o<s))return A.a(q,o)
q[o]=191
r.b=p+1
if(!(p<s))return A.a(q,p)
q[p]=189},
c2(a,b){var s,r,q,p,o,n=this
if((b&64512)===56320){s=65536+((a&1023)<<10)|b&1023
r=n.c
q=n.b
p=n.b=q+1
r.$flags&2&&A.r(r)
o=r.length
if(!(q<o))return A.a(r,q)
r[q]=s>>>18|240
q=n.b=p+1
if(!(p<o))return A.a(r,p)
r[p]=s>>>12&63|128
p=n.b=q+1
if(!(q<o))return A.a(r,q)
r[q]=s>>>6&63|128
n.b=p+1
if(!(p<o))return A.a(r,p)
r[p]=s&63|128
return!0}else{n.aG()
return!1}},
bS(a,b,c){var s,r,q,p,o,n,m,l,k=this
if(b!==c){s=c-1
if(!(s>=0&&s<a.length))return A.a(a,s)
s=(a.charCodeAt(s)&64512)===55296}else s=!1
if(s)--c
for(s=k.c,r=s.$flags|0,q=s.length,p=a.length,o=b;o<c;++o){if(!(o<p))return A.a(a,o)
n=a.charCodeAt(o)
if(n<=127){m=k.b
if(m>=q)break
k.b=m+1
r&2&&A.r(s)
s[m]=n}else{m=n&64512
if(m===55296){if(k.b+4>q)break
m=o+1
if(!(m<p))return A.a(a,m)
if(k.c2(n,a.charCodeAt(m)))o=m}else if(m===56320){if(k.b+3>q)break
k.aG()}else if(n<=2047){m=k.b
l=m+1
if(l>=q)break
k.b=l
r&2&&A.r(s)
if(!(m<q))return A.a(s,m)
s[m]=n>>>6|192
k.b=l+1
s[l]=n&63|128}else{m=k.b
if(m+2>=q)break
l=k.b=m+1
r&2&&A.r(s)
if(!(m<q))return A.a(s,m)
s[m]=n>>>12|224
m=k.b=l+1
if(!(l<q))return A.a(s,l)
s[l]=n>>>6&63|128
k.b=m+1
if(!(m<q))return A.a(s,m)
s[m]=n&63|128}}}return o}}
A.eQ.prototype={
j(a){return this.bP()}}
A.D.prototype={
ga4(){return A.lR(this)}}
A.d0.prototype={
j(a){var s=this.a
if(s!=null)return"Assertion failed: "+A.eb(s)
return"Assertion failed"}}
A.aJ.prototype={}
A.ap.prototype={
gaB(){return"Invalid argument"+(!this.a?"(s)":"")},
gaA(){return""},
j(a){var s=this,r=s.c,q=r==null?"":" ("+r+")",p=s.d,o=p==null?"":": "+A.k(p),n=s.gaB()+q+o
if(!s.a)return n
return n+s.gaA()+": "+A.eb(s.gaQ())},
gaQ(){return this.b}}
A.cg.prototype={
gaQ(){return A.kk(this.b)},
gaB(){return"RangeError"},
gaA(){var s,r=this.e,q=this.f
if(r==null)s=q!=null?": Not less than or equal to "+A.k(q):""
else if(q==null)s=": Not greater than or equal to "+A.k(r)
else if(q>r)s=": Not in inclusive range "+A.k(r)+".."+A.k(q)
else s=q<r?": Valid value range is empty":": Only valid value is "+A.k(r)
return s}}
A.d8.prototype={
gaQ(){return A.j(this.b)},
gaB(){return"RangeError"},
gaA(){if(A.j(this.b)<0)return": index must not be negative"
var s=this.f
if(s===0)return": no indices are valid"
return": index should be less than "+s},
gm(a){return this.f}}
A.cn.prototype={
j(a){return"Unsupported operation: "+this.a}}
A.dB.prototype={
j(a){return"UnimplementedError: "+this.a}}
A.cl.prototype={
j(a){return"Bad state: "+this.a}}
A.d5.prototype={
j(a){var s=this.a
if(s==null)return"Concurrent modification during iteration."
return"Concurrent modification during iteration: "+A.eb(s)+"."}}
A.dq.prototype={
j(a){return"Out of Memory"},
ga4(){return null},
$iD:1}
A.ck.prototype={
j(a){return"Stack Overflow"},
ga4(){return null},
$iD:1}
A.eR.prototype={
j(a){return"Exception: "+this.a}}
A.c2.prototype={
j(a){var s,r,q,p,o,n,m,l,k,j,i,h=this.a,g=""!==h?"FormatException: "+h:"FormatException",f=this.c,e=this.b
if(typeof e=="string"){if(f!=null)s=f<0||f>e.length
else s=!1
if(s)f=null
if(f==null){if(e.length>78)e=B.c.ac(e,0,75)+"..."
return g+"\n"+e}for(r=e.length,q=1,p=0,o=!1,n=0;n<f;++n){if(!(n<r))return A.a(e,n)
m=e.charCodeAt(n)
if(m===10){if(p!==n||!o)++q
p=n+1
o=!1}else if(m===13){++q
p=n+1
o=!0}}g=q>1?g+(" (at line "+q+", character "+(f-p+1)+")\n"):g+(" (at character "+(f+1)+")\n")
for(n=f;n<r;++n){if(!(n>=0))return A.a(e,n)
m=e.charCodeAt(n)
if(m===10||m===13){r=n
break}}l=""
if(r-p>78){k="..."
if(f-p<75){j=p+75
i=p}else{if(r-f<75){i=r-75
j=r
k=""}else{i=f-36
j=f+36}l="..."}}else{j=r
i=p
k=""}return g+l+B.c.ac(e,i,j)+k+"\n"+B.c.ap(" ",f-i+l.length)+"^\n"}else return f!=null?g+(" (at offset "+A.k(f)+")"):g}}
A.h.prototype={
ce(a,b){var s,r
A.A(this).h("h.E(h.E,h.E)").a(b)
s=this.gt(this)
if(!s.l())throw A.f(A.iJ())
r=s.gp()
while(s.l())r=b.$2(r,s.gp())
return r},
K(a,b){var s
A.A(this).h("m(h.E)").a(b)
for(s=this.gt(this);s.l();)if(b.$1(s.gp()))return!0
return!1},
gm(a){var s,r=this.gt(this)
for(s=0;r.l();)++s
return s},
I(a,b){var s,r
A.es(b,"index")
s=this.gt(this)
for(r=b;s.l();){if(r===0)return s.gp();--r}throw A.f(A.ec(b,b-r,this,null,"index"))},
j(a){return A.lJ(this,"(",")")}}
A.cq.prototype={
I(a,b){var s=this.a
if(0>b||b>=s)A.M(A.ec(b,s,this,null,"index"))
return this.b.$1(b)},
gm(a){return this.a}}
A.W.prototype={
j(a){return"MapEntry("+A.k(this.a)+": "+A.k(this.b)+")"}}
A.Y.prototype={
gD(a){return A.x.prototype.gD.call(this,0)},
j(a){return"null"}}
A.x.prototype={$ix:1,
N(a,b){return this===b},
gD(a){return A.ds(this)},
j(a){return"Instance of '"+A.dt(this)+"'"},
gE(a){return A.nG(this)},
toString(){return this.j(this)}}
A.dQ.prototype={
j(a){return""},
$iaX:1}
A.dx.prototype={
gm(a){return this.a.length},
j(a){var s=this.a
return s.charCodeAt(0)==0?s:s}}
A.en.prototype={
j(a){return"Promise was rejected with a value of `"+(this.a?"undefined":"null")+"`."}}
A.hv.prototype={
$1(a){return this.a.aJ(this.b.h("0/?").a(a))},
$S:8}
A.hw.prototype={
$1(a){if(a==null)return this.a.bq(new A.en(a===undefined))
return this.a.bq(a)},
$S:8}
A.iE.prototype={
$1(a){A.j(a)
return a<0||a>63},
$S:3}
A.d7.prototype={}
A.ax.prototype={}
A.bh.prototype={
gm(a){return this.d}}
A.hu.prototype={
$2(a,b){var s=this.a.a,r=16+a
s.$flags&2&&A.r(s)
if(!(r>=0&&r<s.length))return A.a(s,r)
s[r]=b
return b},
$S:9}
A.fz.prototype={
$2(a,b){var s=this.a.a,r=16+a
s.$flags&2&&A.r(s)
if(!(r>=0&&r<s.length))return A.a(s,r)
s[r]=b
return b},
$S:9}
A.ft.prototype={
$0(){return A.d([],t.t)},
$S:40}
A.fu.prototype={
$1(a){return t.R.a(a).a},
$S:42}
A.fv.prototype={
$1(a){return A.j(a)===0},
$S:3}
A.fw.prototype={
$1(a){return!this.a.aj(A.B(a))},
$S:18}
A.fx.prototype={
$1(a){var s
A.j(a)
s=this.a.b?B.e:B.d
if(!(a>=0&&a<s.length))return A.a(s,a)
return s[a]},
$S:46}
A.fy.prototype={
$1(a){t.cz.a(a)
return a.a!==this.a&&a.b.c===this.b.c},
$S:48}
A.fh.prototype={
$1(a){var s=this.a.a,r=16+a
if(!(r<s.length))return A.a(s,r)
if(s[r]!==255){s=this.b
if(s!=null){r=s.a
s=a>=r&&a<r+s.b}else s=!1}else s=!0
return s},
$S:3}
A.hI.prototype={
$3(a,b,c){var s,r,q=this,p=a-q.a,o=b-q.b
if(p>=0&&p<q.c&&o>=0&&o<q.d){s=q.e
r=o*q.c+p
s.$flags&2&&A.r(s)
if(!(r>=0&&r<s.length))return A.a(s,r)
s[r]=c&63}},
$S:10}
A.aa.prototype={}
A.ee.prototype={}
A.a1.prototype={
U(a){return this.b&&a>=28?a+2:a},
aq(a){var s=this.U(a)
return this.b&&this.U(a)>=30?259674+s-30:147017+s},
a1(a){var s=this.U(a)
return this.b&&this.U(a)>=30?147424+8*(s-30):146793+8*s},
b9(a){var s,r,q=this
if(q.b&&q.U(a)>=30)s=null
else{s=q.a.a
r=16+(147045+q.U(a))
if(!(r<s.length))return A.a(s,r)
r=s[r]
s=r}return s},
al(a){var s=this.b9(a),r=this.a
return s==null?r.aa(196301+2*(this.U(a)-30)):r.aa(65536+2*s)},
G(a){var s,r,q,p,o,n,m,l,k,j,i,h,g,f=this,e=f.a,d=e.a,c=16+f.aq(a),b=d.length
if(!(c>=0&&c<b))return A.a(d,c)
s=d[c]
f.b9(a)
r=A.J(B.N,t.S)
q=f.bc(f.al(a),r)
p=f.bc(e.aa(65694),r)
e=A.d([],t.U)
c=A.J(q,t.I)
B.b.C(c,p)
o=c.length
n=0
for(;n<c.length;c.length===o||(0,A.o)(c),++n){m=c[n]
l=m.a
k=r.length
if(l<192){if(0>=k)return A.a(r,0)
k=r[0]}else{if(1>=k)return A.a(r,1)
k=r[1]}e.push(new A.aa(l,k,m.c,m.d,m.e,m.f))}j=f.bV(s)
i=new Uint8Array(A.z(B.Q))
h=f.a1(a)
for(c=i.$flags|0,g=0;g<8;++g){o=B.P[g]
l=16+(h+g)
if(!(l>=0&&l<b))return A.a(d,l)
l=d[l]
c&2&&A.r(i)
if(!(o<i.length))return A.a(i,o)
i[o]=l}d=f.b?B.e:B.d
if(!(a<d.length))return A.a(d,a)
d=r.length
if(0>=d)return A.a(r,0)
if(1>=d)return A.a(r,1)
return new A.ee(j.a,j.b,e,i,B.O)},
bc(a,b){var s,r,q,p,o,n,m,l,k,j
t.L.a(b)
s=A.jO(a)
for(r=this.a.a,q=r.length,p=0,o=0;;){n=s+1
m=16+s
if(!(m>=0&&m<q))return A.a(r,m)
l=r[m]
if(l===246)for(s=n,k=0;k<4;++k,s=n){n=s+1
m=16+s
if(!(m>=0&&m<q))return A.a(r,m)
j=r[m]
if(j!==255&&k<2)B.b.q(b,k,j)}else if(l===252)s=n+1
else{if(l===248){s=n+1
m=16+n
if(!(m>=0&&m<q))return A.a(r,m)
o=r[m]
n=s+1
m=16+s
if(!(m>=0&&m<q))return A.a(r,m)
p=r[m]}else if(l>=128&&l<240){m=16+n
if(!(m>=0&&m<q))return A.a(r,m)
return this.bW(65536+((l<<8|r[m])&32767),p,o)}else throw A.f(A.aE("Unknown sprite group command $"+B.a.k(l,16)+" (header $"+B.a.k(a,16)+")",null,null))
s=n}}},
bW(a,b,c){var s,r,q,p,o,n,m,l,k,j,i,h,g=A.d([],t.U)
for(s=this.a.a,r=s.length,q=2*c,p=2*b,o=0,n=0,m=0;;){l=a+1
k=16+a
if(!(k<r))return A.a(s,k)
j=s[k]
if(j===255)return g
if(j===252){a=l+1
k=16+l
if(!(k<r))return A.a(s,k)
i=s[k]
k=i>>>4
if(k===2){m=i&15
if(m>=8)m-=16}else if(k>=3){n=i&15
if(n>=8)n-=16
o+=n}}else if(j>=128){k=j&31
o=p+8*(k>=16?k-32:k)+n+-60
a=l}else{a=l+1
k=16+l
if(!(k<r))return A.a(s,k)
k=s[k]
h=j>>>2&15
if(h>=8)h-=16
B.b.i(g,new A.aa(k|128,0,o&255,q+8*h+m+-59&255,j&3|2,(j&64)!==0))}}},
bV(a){var s,r,q,p,o,n,m,l,k,j,i,h=this.a,g=h.aa(20480+2*a),f=t.S,e=A.aU(64,0,!1,f),d=A.aU(16,0,!1,f)
for(h=h.a,f=h.length,g=16384+g-40960+7,s=0;s<4;++s){r=16+(g+s)
if(!(r>=0&&r<f))return A.a(h,r)
q=90112+17*h[r]
r=16+q
if(!(r<f))return A.a(h,r)
p=h[r]
o=(s>>>1)*4
n=(s&1)*4
for(r=q+1,m=0;m<16;++m){l=B.a.u(m,4)
k=B.a.A(m,4)
j=16+(r+m)
if(!(j<f))return A.a(h,j)
B.b.q(e,(o+l)*8+n+k,h[j])}for(i=0;i<4;++i)B.b.q(d,(B.a.u(o,2)+B.a.u(i,2))*4+B.a.u(n,2)+B.a.A(i,2),B.a.a_(p,2*i)&3)}return new A.i(e,d)}}
A.ej.prototype={}
A.c9.prototype={}
A.fN.prototype={
$1(a){var s,r,q,p,o,n,m,l
t.w.a(a)
s=this.a
r=t.s
q=A.d([],r)
p=s.length
o=32-p
n=a.length
if(n>o)B.b.i(q,"Logo needs "+n+" sprites but the helmet has room for "+o+" (the facemask uses "+p+" of 32).")
m=A.kl(a,s)
l=m.b
if(l>8)B.b.i(q,"Row "+(m.a-32)+" would need "+l+" sprites side by side (logo + facemask); the NES shows only 8 per line. Make the logo narrower there.")
if(q.length===0){s=this.b
r=this.c
r=A.l_(s,r,A.ne(s,r,a),!1,!0,!1)
s=r}else s=new A.ax(q,A.d([],r),A.d([],r),null)
return s},
$S:20}
A.fO.prototype={
$3(a,b,c){var s,r,q,p,o,n,m,l
t.w.a(a)
t.be.a(b)
s=a.length
r=this.a
q=r.length
r=A.kl(a,r)
p=A.a4(t.N)
for(o=a.length,n=0;n<a.length;a.length===o||(0,A.o)(a),++n)p.i(0,B.h.W(a[n].c))
p=p.a
o=A.d([],t.n)
for(m=a.length,n=0;n<a.length;a.length===m||(0,A.o)(a),++n){l=a[n]
o.push(new A.i(l.a-32,l.b-32))}return new A.c9(new A.ej(s,32-q,r.b,p,o),b,c)},
$S:21}
A.hp.prototype={
$2(a,b){var s=this.a
return A.lX(s,0,A.fP(this.b,"count",t.S),A.Q(s).c).K(0,new A.hq(a,b))},
$S:5}
A.hq.prototype={
$1(a){var s,r,q
t.R.a(a)
s=this.a
r=a.a
q=!1
if(s>=r)if(s<r+8){s=this.b
r=a.b
s=s>=r&&s<r+8}else s=q
else s=q
return s},
$S:6}
A.hr.prototype={
$2(a,b){return a>=0&&a<72&&b>=0&&b<72},
$S:5}
A.a5.prototype={}
A.fl.prototype={
$0(){var s=this.b,r=B.b.bs(s,new A.fk(this.a))
return r<0?null:B.b.ao(s,r)},
$S:24}
A.fk.prototype={
$1(a){var s
t.I.a(a)
s=this.a
return a.c===s.b&&a.d===s.a},
$S:25}
A.fo.prototype={
$2(a,b){return B.b.K(this.a,new A.fp(a,b))},
$S:5}
A.fp.prototype={
$1(a){var s,r,q
t.R.a(a)
s=this.a
r=a.a
q=!1
if(s>=r)if(s<r+8){s=this.b
r=a.b
s=s>=r&&s<r+8}else s=q
else s=q
return s},
$S:6}
A.fm.prototype={
$1(a){return A.j(a)!==0},
$S:3}
A.fq.prototype={
$1$keep(a){var s,r,q,p,o,n,m=this,l=A.a4(t.S),k=A.d([],t.dc)
for(s=m.a,r=m.b,q=m.c,p=m.d,o=m.e,n=0;n<s.length;++n)k.push(new A.fs(s,n,r,a,q,p,o,l).$0())
return k},
$S:26}
A.fs.prototype={
$0(){var s,r,q,p,o,n,m,l,k,j,i,h,g=this,f=g.a,e=g.b
if(!(e<f.length))return A.a(f,e)
s=f[e]
r=s.a
q=s.b
p=J.iM(64,t.R)
for(o=0;o<64;++o)p[o]=new A.i(r+B.a.A(o,8),q+B.a.u(o,8))
f=g.c
if(!(e<f.length))return A.a(f,e)
n=f[e]
if(g.d&&n!=null&&B.b.M(p,new A.fr(g.e,g.f)))m=A.kD(g.r,n)
else{f=A.d([],t.t)
for(e=g.f,l=g.w,k=0;k<64;++k){j=p[k]
i=j.b
h=j.a
f.push(l.J(0,i*256+h)?0:A.fb(e,h,i))}m=new Uint8Array(A.z(f))}for(f=m.length,e=g.w,o=0;o<64;++o){if(!(o<f))return A.a(m,o)
if(m[o]!==0){l=p[o]
e.i(0,l.b*256+l.a)}}return m},
$S:11}
A.fr.prototype={
$1(a){var s,r
t.R.a(a)
s=a.a
r=a.b
return A.fb(this.a,s,r)===A.fb(this.b,s,r)},
$S:6}
A.fn.prototype={
$1(a){return A.j(a)!==0},
$S:3}
A.fi.prototype={
$1(a){var s,r,q,p,o,n
for(s=this.a,r=s+8,q=this.b,p=a*72,o=q.length;s<r;++s){n=!1
if(s>=0)if(s<72){n=p+s
if(!(n<o))return A.a(q,n)
n=q[n]!==0}if(n)return!0}return!1},
$S:3}
A.fe.prototype={
$1(a){return t.bY.a(a).b},
$S:28}
A.ff.prototype={
$1(a){return t.I.a(a).d},
$S:29}
A.fF.prototype={
$0(){var s=this.a,r=s.a-128+60,q=s.b-128+59,p=this.b,o=p.a2(B.h.W(s.c),new A.fE(this.c,p,s))
return new A.cy([B.a.B(r,3),r&7,q&7,o,B.a.B(q,3)])},
$S:30}
A.fE.prototype={
$0(){var s=this.b
this.a.q(0,s.a,A.mw(this.c.c))
return s.a},
$S:14}
A.h1.prototype={
$2(a,b){var s,r=B.f[b&63],q=this.a,p=a*4
q.$flags&2&&A.r(q)
if(!(p<24192))return A.a(q,p)
q[p]=r>>>16
s=p+1
if(!(s<24192))return A.a(q,s)
q[s]=r>>>8&255
s=p+2
if(!(s<24192))return A.a(q,s)
q[s]=r&255
p+=3
if(!(p<24192))return A.a(q,p)
q[p]=255},
$S:9}
A.ei.prototype={}
A.hD.prototype={
$1(a){var s,r,q=this.a,p=a*4,o=q.length
if(!(p<o))return A.a(q,p)
s=q[p]
r=p+1
if(!(r<o))return A.a(q,r)
r=q[r]
p+=2
if(!(p<o))return A.a(q,p)
return(s<<16|r<<8|q[p])>>>0},
$S:4}
A.hC.prototype={
$1(a){var s=this.a,r=a*4+3
if(!(r<s.length))return A.a(s,r)
return s[r]>=128},
$S:3}
A.hB.prototype={
$0(){var s,r,q,p,o,n,m,l=t.S,k=A.ac(l,l)
for(l=this.a,s=24*l,l=24*(l+1),r=this.b,q=this.c,p=74;p<84;++p)for(o=p*72,n=s;n<l;++n){m=o+n
if(r.$1(m))k.cn(q.$1(m),new A.hy(),new A.hz())}if(k.a===0)return null
return new A.N(k,k.$ti.h("N<1,2>")).ce(0,new A.hA()).a},
$S:33}
A.hy.prototype={
$1(a){return A.j(a)+1},
$S:4}
A.hz.prototype={
$0(){return 1},
$S:14}
A.hA.prototype={
$2(a,b){var s=t.aS
s.a(a)
s.a(b)
return b.b>a.b?b:a},
$S:34}
A.em.prototype={
aa(a){var s,r=this.a,q=16+a,p=r.length
if(!(q>=0&&q<p))return A.a(r,q)
q=r[q]
s=16+(a+1)
if(!(s>=0&&s<p))return A.a(r,s)
return(q|r[s]<<8)>>>0},
a3(a,b){var s,r,q,p,o,n,m,l=this.a,k=16+this.b+a*1024,j=l.BYTES_PER_ELEMENT,i=A.ch(k,k+1024,B.a.bF(l.byteLength,j)),h=J.iG(B.h.gaI(l),l.byteOffset+k*j,(i-k)*j),g=(b&63)*16
k=A.d([],t.t)
for(l=h.length,s=g+8,r=0;r<8;++r)for(q=g+r,p=s+r,o=0;o<8;++o){if(!(q<l))return A.a(h,q)
n=7-o
m=B.a.bf(h[q],n)
if(!(p<l))return A.a(h,p)
k.push(m&1|(B.a.bf(h[p],n)&1)<<1)}return k}}
A.dv.prototype={}
A.eu.prototype={}
A.bc.prototype={
ai(a){return this.b.b&&a>=28?a+2:a},
ck(a){var s,r,q,p=this.ai(a)
if(this.b.b&&this.ai(a)>=30){s=2*(p-30)
r=259763+s
s=259771+s
return A.d([r,r+1,s,s+1],t.t)}s=A.d([],t.t)
for(r=146358+5*p,q=0;q<4;++q)s.push(r+q)
return s},
br(a){var s=this.ai(a)
return this.b.b&&this.ai(a)>=30?259779+s-30:146358+5*s+4},
aX(a){var s,r,q,p=this.b.b,o=p?16:14,n=a>=o,m=n?a-o:a
if(p){p=B.a.B(m,2)
s=n?18:8
return new A.i(1+8*p,s+2*(m&3))}p=t.t
r=n?A.d([5,5,4],p):A.d([5,4,5],p)
q=0
for(;;){if(!(q<3))return A.a(r,q)
p=r[q]
if(!(m>=p))break;++q
m-=p}p=n?18:6
return new A.i(2+10*q,p+2*m)},
c4(a){var s,r,q,p,o,n,m,l=this.aX(a),k=l.a,j=l.b,i=a<16?j-2:j,h=A.d([],t.n)
for(s=[new A.i(16954,j),new A.i(17018,i)],r=0;r<2;++r){q=s[r]
p=q.a
o=q.b
for(n=0;n<4;++n){q=o+(n>>>1)
m=k+(n&1)
h.push(new A.i(p+B.a.B(q,2)*8+(m>>>2),(B.a.B(q,1)&1)*4+(m>>>1&1)*2))}}return h},
ad(a,b){var s=this.a.a,r=16+(16954+B.a.B(b,2)*8+(a>>>2))
if(!(r<s.length))return A.a(s,r)
return B.a.a_(s[r],(B.a.B(b,1)&1)*4+(a>>>1&1)*2)&3},
G(a){var s,r,q,p,o,n,m,l,k,j,i,h,g,f,e=this,d=e.a.a,c=16+e.br(a),b=d.length
if(!(c>=0&&c<b))return A.a(d,c)
s=d[c]
r=s>>>2
q=new Uint8Array(32)
for(p=0;p<16;++p){c=16+(106816+p)
if(!(c<b))return A.a(d,c)
c=d[c]
if(!(p<32))return A.a(q,p)
q[p]=c
c=16+p
o=16+(108592+p)
if(!(o<b))return A.a(d,o)
o=d[o]
if(!(c<32))return A.a(q,c)
q[c]=o}n=e.aX(a)
m=n.a
l=n.b
c=e.b.b
o=c?B.e:B.d
if(!(a<o.length))return A.a(o,a)
o=o[a]
k=t.t
j=A.d([],k)
for(i=e.ck(a),h=i.length,g=0;g<i.length;i.length===h||(0,A.o)(i),++g){f=16+i[g]
if(!(f>=0&&f<b))return A.a(d,f)
j.push(d[f])}if(c){d=m+1
c=l+1
k=A.d([e.ad(m,l),e.ad(d,l),e.ad(m,c),e.ad(d,c)],k)
d=k}else d=A.aU(4,s&3,!1,t.S)
return new A.eu(o,j,d,r===0?B.R:e.bU(r),q)},
bU(a){var s,r,q,p,o,n,m,l=146497+a,k=this.a.a,j=16+l,i=k.length
if(!(j<i))return A.a(k,j)
s=k[j]
l=16+(l+1)
if(!(l<i))return A.a(k,l)
r=k[l]
q=new A.eE()
l=A.d([],t.W)
p=s
for(;;){if(!(p===s||p<r))break
j=146513+p
o=16+(j+3)
if(!(o<i))return A.a(k,o)
o=q.$1(k[o])
n=16+j
if(!(n<i))return A.a(k,n)
n=q.$1(k[n])
m=16+(j+1)
if(!(m<i))return A.a(k,m)
m=k[m]
j=16+(j+2)
if(!(j<i))return A.a(k,j)
l.push(new A.dv(o,n,m,k[j]))
p+=4}return l}}
A.eE.prototype={
$1(a){return a>=128?a-256:a},
$S:4}
A.eD.prototype={}
A.er.prototype={}
A.aM.prototype={
aM(a,b){var s=this.a,r=!1
if(a>=s)if(a<s+8){s=this.b
s=b>=s&&b<s+8}else s=r
else s=r
return s},
aU(a,b){var s=this,r=a-s.a,q=b-s.b,p=s.e?7-q:q,o=s.d?7-r:r
return p*8+o}}
A.ev.prototype={
gY(){var s=this.x
if(s===$){s=A.J(this.d.c,t.S)
this.x=s}return s},
bG(a,b){var s,r,q,p,o,n,m,l,k,j,i,h=this
for(s=h.d,r=s.b,q=r.length,p=h.e,o=0;o<r.length;r.length===q||(0,A.o)(r),++o){n=r[o]
if(n===0)m=null
else{m=n>>>6
if(!(m<4))return A.a(B.k,m)
m=B.k[m]*64+(n&63)}B.b.i(p,m)}for(s=s.e,r=s.length,q=h.f,o=0;o<s.length;s.length===r||(0,A.o)(s),++o){l=s[o]
m=l.c
k=m>>>6
if(!(k<4))return A.a(B.j,k)
j=l.d
B.b.i(q,new A.aM(4+l.a,1+l.b,B.j[k]*64+(m&63),(j&64)!==0,(j&128)!==0,j&3))}s=A.J(new A.bA(p,t.d),t.S)
r=A.Q(q)
B.b.C(s,new A.ae(q,r.h("b(1)").a(new A.ez()),r.h("ae<1,b>")))
r=s.length
q=h.r
p=h.w
o=0
for(;o<s.length;s.length===r||(0,A.o)(s),++o){i=s[o]
q.a2(i,new A.eA(h,i))
p.a2(i,new A.eB(h,i))}},
ga5(){var s,r,q,p,o,n,m,l,k,j,i,h=new Uint8Array(256)
for(s=this.r,r=this.e,q=0;q<4;++q){if(!(q<r.length))return A.a(r,q)
p=r[q]
if(p==null)continue
o=s.v(0,p)
for(n=(q>>>1)*8,m=(q&1)*8,l=o.length,k=0;k<64;++k){j=(n+B.a.u(k,8))*16+m+B.a.A(k,8)
if(!(k<l))return A.a(o,k)
i=o[k]
if(!(j<256))return A.a(h,j)
h[j]=i}}return h},
T(){var s,r,q,p,o,n,m,l,k,j,i,h,g,f,e,d,c,b,a,a0=new Uint8Array(432),a1=new Uint8Array(432)
for(s=this.f,r=s.length,q=this.r,p=0;p<s.length;s.length===r||(0,A.o)(s),++p){o=s[p]
n=q.v(0,o.c)
for(m=o.b,l=m+8,k=o.a,j=k+8,i=n.length,h=o.f;m<l;++m)for(g=m*24,f=m>=0,e=m>=18,d=k;d<j;++d){if(d<0||!f||d>=24||e)continue
c=o.aU(d,m)
if(!(c>=0&&c<i))return A.a(n,c)
b=n[c]
a=g+d
if(b!==0){if(!(a>=0&&a<432))return A.a(a0,a)
c=a0[a]===0}else c=!1
if(c){if(!(a>=0&&a<432))return A.a(a0,a)
a0[a]=b
if(!(a<432))return A.a(a1,a)
a1[a]=h}}}return new A.i(a0,a1)},
aH(){var s,r,q,p,o,n,m,l=this,k=l.ga5(),j=new Uint8Array(256)
for(s=l.d,r=s.f,s=s.c,q=t.S,p=0;p<256;++p){o=(B.a.u(p,16)>>>3)*2+(B.a.A(p,16)>>>3)
if(k[p]===0)n=255
else{m=l.x
if(m===$){m=A.J(s,q)
l.x=m}if(!(o<m.length))return A.a(m,o)
n=m[o]*4+k[p]
if(!(n>=0&&n<32))return A.a(r,n)
n=r[n]}if(!(p<256))return A.a(j,p)
j[p]=n}return j},
aT(){var s,r,q,p,o=this.T(),n=o.a,m=o.b,l=A.d([],t.t)
for(s=this.d.f,r=m.length,q=0;q<432;++q){p=n[q]
if(p===0)p=255
else{if(!(q<r))return A.a(m,q)
p=16+m[q]*4+p
if(!(p<32))return A.a(s,p)
p=s[p]}l.push(p)}return new Uint8Array(A.z(l))},
bu(a,b){var s,r,q,p
for(s=this.f,r=s.length,q=0;q<r;++q){p=s[q]
if(p.aM(a,b))return p.f}return r===0?0:B.b.ga0(s).f},
ga9(){var s,r,q,p,o=A.d([],t.n)
for(s=this.f,r=s.length,q=0;q<s.length;s.length===r||(0,A.o)(s),++q){p=s[q]
o.push(new A.i(p.a,p.b))}return o},
O(a,b){var s,r=!1
if(a>=0)if(b>=0)if(a<16)if(b<16){r=this.e
s=B.a.B(b,3)*2+B.a.B(a,3)
if(!(s<r.length))return A.a(r,s)
s=r[s]!=null
r=s}return r},
aS(a,b){return B.b.K(this.f,new A.eC(a,b))},
am(a,b,c){var s,r
if(!this.O(a,b))return!1
s=this.e
r=B.a.B(b,3)*2+B.a.B(a,3)
if(!(r<s.length))return A.a(s,r)
r=s[r]
r.toString
r=this.r.v(0,r)
r.toString
s=(b&7)*8+(a&7)
r.$flags&2&&A.r(r)
if(!(s<r.length))return A.a(r,s)
r[s]=c
return!0},
an(a,b,c){var s,r,q,p,o,n,m=this,l=A.d([],t.A)
for(s=m.f,r=s.length,q=0;q<s.length;s.length===r||(0,A.o)(s),++q){p=s[q]
if(p.aM(a,b))l.push(p)}s=l.length
if(s===0)return!1
if(c===0)for(r=m.r,q=0;q<l.length;l.length===s||(0,A.o)(l),++q){p=l[q]
o=r.v(0,p.c)
o.toString
n=p.aU(a,b)
o.$flags&2&&A.r(o)
if(!(n>=0&&n<o.length))return A.a(o,n)
o[n]=0}else{s=m.T().a
r=b*24+a
if(!(r>=0&&r<432))return A.a(s,r)
if(s[r]!==c){s=m.r.v(0,B.b.ga0(l).c)
s.toString
l=B.b.ga0(l).aU(a,b)
s.$flags&2&&A.r(s)
if(!(l>=0&&l<s.length))return A.a(s,l)
s[l]=c}}return!0},
bm(a){var s,r
for(s=0,r=0;r<256;++r)if(!this.am(B.a.A(r,16),B.a.u(r,16),a[r])&&a[r]!==0)++s
return s},
bn(a){var s,r,q,p=this.T().a
for(s=0,r=0;r<432;++r){q=a[r]
if(q===p[r])continue
if(!this.an(B.a.A(r,24),B.a.u(r,24),q)&&a[r]!==0)++s}return s},
c3(a){var s=this,r=A.d([],t.s),q=new A.er(r),p=t.S,o=a.e
if(s.c.b.b)p=A.J(o,p)
else{if(0>=o.length)return A.a(o,0)
p=A.aU(4,o[0],!1,p)}s.x=t.L.a(p)
p=q.a=s.bm(a.b)
if(p>0){o=p===1?"":"s"
B.b.i(r,""+p+" background pixel"+o+" had no tile to go in (this helmet has empty tile slots there), so they were left out.")}if(!a.d){if(s.f.length!==0)B.b.i(r,a.a+" has no overlay sprites, so this helmet's overlay was left as it is.")}else{p=a.c
if(s.f.length===0){o=A.a6(p)
o=new A.aL(p,o.h("m(v.E)").a(new A.ey()),o.h("aL<v.E>")).gm(0)
q.b=o
B.b.i(r,"This helmet has no overlay sprites, so "+a.a+"'s overlay ("+o+" pixels) was left out.")}else{p=q.b=s.bn(p)
if(p>0){o=p===1?"":"s"
B.b.i(r,""+p+" overlay pixel"+o+" fell outside this helmet's sprites, so they were left out.")}}}return q},
bC(){var s,r,q=t.S,p=A.ac(q,t.p)
for(s=this.r,s=new A.N(s,A.A(s).h("N<1,2>")).gt(0);s.l();){r=s.d
p.q(0,r.a,new Uint8Array(A.z(r.b)))}q=A.J(this.gY(),q)
return new A.i(p,q)},
bw(a){var s,r,q,p
t.J.a(a)
for(s=a.a,s=new A.N(s,A.A(s).h("N<1,2>")).gt(0),r=this.r;s.l();){q=s.d
p=r.v(0,q.a)
p.toString
B.h.bB(p,0,q.b)}s=A.J(a.b,t.S)
this.x=t.L.a(s)},
gS(){var s,r,q,p,o,n,m=A.a4(t.S)
for(s=this.r,s=new A.N(s,A.A(s).h("N<1,2>")).gt(0),r=this.w;s.l();){q=s.d
p=q.b
o=q.a
n=r.v(0,o)
n.toString
if(!A.lV(p,n))m.i(0,o)}return m},
gZ(){var s,r,q,p
for(s=this.d.c,r=t.S,q=0;q<4;++q){p=this.x
if(p===$){p=A.J(s,r)
this.x=p}if(!(q<p.length))return A.a(p,q)
if(p[q]!==s[q])return!0}return!1},
gbl(){var s,r,q,p,o,n,m,l,k,j=this,i=j.gS()
if(i.a===0)return B.S
s=j.y
if(s==null)s=j.y=j.c1()
r=A.a4(t.S)
for(q=A.k6(i,i.r,A.A(i).c),p=q.$ti.c;q.l();){o=q.d
o=s.v(0,o==null?p.a(o):o)
r.C(0,o==null?B.p:o)}r.aV(0,j.b)
q=A.d([],t.s)
p=A.J(r,r.$ti.c)
B.b.bD(p)
o=p.length
n=j.c.b
m=0
for(;m<p.length;p.length===o||(0,A.o)(p),++m){l=p[m]
k=n.b?B.e:B.d
if(!(l>=0&&l<k.length))return A.a(k,l)
q.push(k[l])}return q},
c1(){var s,r,q,p,o,n,m,l=A.ac(t.S,t.f),k=this.c,j=k.b.b,i=0
for(;;){if(!(i<(j?B.e:B.d).length))break
s=k.G(i)
for(r=s.b,q=r.length,p=0;p<r.length;r.length===q||(0,A.o)(r),++p){o=r[p]
if(o!==0){n=o>>>6
if(!(n<4))return A.a(B.k,n)
l.a2(B.k[n]*64+(o&63),new A.ew()).i(0,i)}}for(r=s.e,q=r.length,p=0;p<r.length;r.length===q||(0,A.o)(r),++p){n=r[p].c
m=n>>>6
if(!(m<4))return A.a(B.j,m)
l.a2(B.j[m]*64+(n&63),new A.ex()).i(0,i)}++i}return l},
bp(){var s,r,q,p,o,n,m,l,k,j,i,h,g,f,e,d,c,b,a,a0,a1=this,a2=a1.a,a3=new Uint8Array(A.z(a2.a))
for(s=a1.gS(),s=A.k6(s,s.r,A.A(s).c),a2=16+a2.b,r=a1.r,q=s.$ti.c,p=a3.$flags|0;s.l();){o=s.d
if(o==null)o=q.a(o)
n=a2+B.a.B(o,6)*1024+(o&63)*16
m=r.v(0,o)
for(o=n+8,l=m.length,k=0;k<8;++k){for(j=k*8,i=0,h=0,g=0;g<8;++g){f=j+g
if(!(f<l))return A.a(m,f)
f=m[f]
e=7-g
i=(i|B.a.ab(f&1,e))>>>0
h=(h|B.a.ab(f>>>1&1,e))>>>0}j=n+k
p&2&&A.r(a3)
f=a3.length
if(!(j<f))return A.a(a3,j)
a3[j]=i
j=o+k
if(!(j<f))return A.a(a3,j)
a3[j]=h}}if(a1.gZ()){a2=a1.c
s=a1.b
if(a2.b.b){d=a2.c4(s)
for(a2=a3.length,s=a1.d.c,r=t.S,c=0;c<d.length;++c){b=d[c]
a=b.b
q=16+b.a
if(!(q>=0&&q<a2))return A.a(a3,q)
o=a3[q]
l=B.a.be(3,a)
a0=a1.x
if(a0===$){a0=A.J(s,r)
a1.x=a0}j=B.a.A(c,4)
if(!(j<a0.length))return A.a(a0,j)
j=B.a.be(a0[j],a)
p&2&&A.r(a3)
a3[q]=(o&~l|j)>>>0}}else{n=16+a2.br(s)
if(!(n>=0&&n<a3.length))return A.a(a3,n)
a2=a3[n]
s=a1.gY()
if(0>=s.length)return A.a(s,0)
s=s[0]
p&2&&A.r(a3)
a3[n]=(a2&4294967292|s)>>>0}}return a3}}
A.ez.prototype={
$1(a){return t.B.a(a).c},
$S:35}
A.eA.prototype={
$0(){var s=this.b
return new Uint8Array(A.z(this.a.a.a3(B.a.B(s,6),s&63)))},
$S:11}
A.eB.prototype={
$0(){var s=this.a.r.v(0,this.b)
s.toString
return new Uint8Array(A.z(s))},
$S:11}
A.eC.prototype={
$1(a){return t.B.a(a).aM(this.a,this.b)},
$S:36}
A.ey.prototype={
$1(a){return A.j(a)!==0},
$S:3}
A.ew.prototype={
$0(){return A.a4(t.S)},
$S:15}
A.ex.prototype={
$0(){return A.a4(t.S)},
$S:15}
A.iz.prototype={
$3(a,b,c){var s,r,q,p
if(c===255)return
s=B.f[c&63]
r=(b*24+a)*4
q=this.a
q.$flags&2&&A.r(q)
if(!(r<3456))return A.a(q,r)
q[r]=s>>>16
p=r+1
if(!(p<3456))return A.a(q,p)
q[p]=s>>>8&255
p=r+2
if(!(p<3456))return A.a(q,p)
q[p]=s&255
p=r+3
if(!(p<3456))return A.a(q,p)
q[p]=255},
$S:10}
A.eF.prototype={}
A.hH.prototype={
$2(a,b){var s,r,q,p=(b*24+a)*4,o=this.a,n=o.length
if(!(p<n))return A.a(o,p)
s=o[p]
r=p+1
if(!(r<n))return A.a(o,r)
r=o[r]
q=p+2
if(!(q<n))return A.a(o,q)
return(s<<16|r<<8|o[q])>>>0},
$S:38}
A.hG.prototype={
$2(a,b){var s=this.a,r=(b*24+a)*4+3
if(!(r<s.length))return A.a(s,r)
return s[r]>=128},
$S:5}
A.hF.prototype={
$3(a,b,c){var s,r,q,p,o,n,m,l,k,j,i,h
if(c!==0){s=this.a.d.f
r=b+c
if(!(r>=0&&r<32))return A.a(s,r)
r=B.f[s[r]&63]===a
s=r}else s=!1
if(s)return c
for(s=this.a.d.f,r=a>>>16,q=a>>>8&255,p=a&255,o=1,n=1073741824,m=1;m<4;++m){l=b+m
if(!(l>=0&&l<32))return A.a(s,l)
l=B.f[s[l]&63]
k=r-(l>>>16)
j=q-(l>>>8&255)
i=p-(l&255)
h=k*k+j*j+i*i
if(h<n){n=h
o=m}}return o},
$S:39}
A.hE.prototype={
$2(a,b){return!this.a.$2(a,b)||J.S(this.b.$2(a,b),this.c)},
$S:5}
A.fU.prototype={
$3(a,b,c){var s,r,q=this
if(a>=0&&a<q.a&&b>=0&&b<q.b){s=q.c
r=b*q.a+a
s.$flags&2&&A.r(s)
if(!(r>=0&&r<432))return A.a(s,r)
s[r]=c&63}},
$S:10}
A.dA.prototype={
bP(){return"ThemeStyle."+this.b}}
A.p.prototype={}
A.ho.prototype={
$1(a){var s=B.a.a_(this.a,a)&255
return B.a.V(B.m.cf(s+((B.a.B(this.b,a)&255)-s)*this.c),0,255)},
$S:4}
A.fj.prototype={
$1(a){return A.e_(a,this.a,4.5)},
$S:4}
A.fg.prototype={
$1(a){var s,r,q
for(s=a,r=0;r<8;++r){q=s>>>1
s=(s&1)!==0?(3988292384^q)>>>0:q}return s},
$S:4}
A.iC.prototype={
$2(a,b){return a.i(0,A.d([b&255,b>>>8&255],t.t))},
$S:16}
A.iD.prototype={
$2(a,b){return a.i(0,A.d([b&255,b>>>8&255,b>>>16&255,b>>>24&255],t.t))},
$S:16}
A.iB.prototype={
$1(a){var s,r,q=this,p=q.b
p.$2(a,20)
p.$2(a,2048)
p.$2(a,0)
p.$2(a,0)
p.$2(a,33)
s=q.c
s.$2(a,q.d)
r=q.a
s.$2(a,r.a.length)
s.$2(a,r.a.length)
p.$2(a,q.e.length)
p.$2(a,0)},
$S:41}
A.ep.prototype={
scd(a){this.b=t.cM.a(a)}}
A.ht.prototype={
$1(a){A.c(a)
return this.a.$0()},
$S:2}
A.h3.prototype={
$0(){return this.a.click()},
$S:0}
A.h4.prototype={
$0(){return A.nv(this.a)},
$S:0}
A.h5.prototype={
$1(a){var s,r,q
A.c(a)
s=this.a
r=A.q(s.files)
q=r==null?null:A.q(r.item(0))
if(q!=null)A.cP(this.b,q)
s.value=""},
$S:1}
A.hf.prototype={
$1(a){A.c(a)
a.preventDefault()
a.stopPropagation()
A.dW()
A.c(this.a.classList).add("over")},
$S:1}
A.hg.prototype={
$1(a){A.c(a)
return A.c(this.a.classList).remove("over")},
$S:2}
A.hh.prototype={
$1(a){var s,r
A.c(a)
a.preventDefault()
a.stopPropagation()
A.dW()
s=A.q(a.dataTransfer)
r=s==null?null:A.q(A.c(s.files).item(0))
if(r!=null)A.cP(this.a,r)},
$S:1}
A.hi.prototype={
$1(a){var s
A.c(a).preventDefault()
A.dW()
s=$.aC()
if(!(s.b==null)){s=$.t()
s=s.b==null?s:null}if(s!=null)A.c(A.e(s.a+"Pane",t.m).classList).add("over")},
$S:1}
A.hj.prototype={
$1(a){if(A.q(A.c(a).relatedTarget)==null)A.dW()},
$S:1}
A.hk.prototype={
$1(a){var s,r,q
A.c(a)
a.preventDefault()
A.dW()
s=A.q(a.dataTransfer)
r=s==null?null:A.q(A.c(s.files).item(0))
if(r==null)return
q=$.aC()
if(!(q.b==null)){s=$.t()
q=s.b==null?s:null}if(q!=null)A.cP(q,r)
else A.e("hint",t.m).textContent="Both ROMs are loaded. Drop the file onto the Source or Target pane to replace that one."},
$S:1}
A.hl.prototype={
$0(){var s=$.t(),r=s.b
if(r==null)return
A.l3($.aC(),new Uint8Array(A.z(r.a.a)),s.c+" (target, with edits)")},
$S:0}
A.hm.prototype={
$1(a){A.c(a)
return A.jv()},
$S:2}
A.h6.prototype={
$0(){var s=$.cR
A.au()
if(s!=null)A.kI(s.a,s.b)},
$S:0}
A.h7.prototype={
$0(){var s=$.cR
A.au()
if(s!=null)A.kX(s.b)},
$S:0}
A.h8.prototype={
$0(){var s,r,q=$.cR
A.au()
if(q!=null){s=q.b
A.bM()
A.bL()
A.bN()
$.dX=s
r=$.t().b
r.toString
$.b_=A.cO(r,s)
$.cM=0
A.io()
A.e("colorEd",t.m).scrollIntoView()}},
$S:0}
A.h9.prototype={
$0(){var s,r=$.t().b
r.toString
s=$.dX
s.toString
$.b_=A.cO(r,s)
A.io()},
$S:0}
A.ha.prototype={
$0(){var s,r,q,p,o=$.cR
A.au()
if(o!=null){s=o.b
A.bM()
A.bK()
A.bN()
$.b1=s
r=$.t()
q=r.b
q.toString
q=A.jo(q,s)
$.nU=q
$.cQ=new Uint8Array(A.z(q))
q=r.b
q.toString
$.kQ=A.jf(q,s,$.iF(),null).a.e
$.aA=$.an=null
q=r.b
q.toString
q=B.b.b0(A.cO(q,s),5)
$.h2=q
q=A.J(q,t.S)
$.a0=q
$.e2=!1
B.b.P($.bp)
$.e1=null
q=t.m
A.e("logoEd",q).hidden=!1
p=A.e("leTitle",q)
r=r.b.b?B.e:B.d
if(!(s<r.length))return A.a(r,s)
p.textContent=r[s]+" logo"
A.az()
A.e("logoEd",q).scrollIntoView()}},
$S:0}
A.hb.prototype={
$1(a){var s,r,q
A.c(a)
s=t.m
r=A.e("ctx",s)
q=A.e("ctxS",s)
if(!A.F(r.contains(A.q(a.target)))&&!A.F(q.contains(A.q(a.target))))A.au()},
$S:1}
A.hc.prototype={
$1(a){var s,r,q,p="HTMLInputElement"
A.c(a)
if(A.B(a.key)==="Escape")A.au()
s=A.q(a.target)
if(s==null)s=null
else s=A.iL(s,p)
if(A.B(a.key)==="?"&&s!==!0&&!A.F(a.ctrlKey)&&!A.F(a.metaKey)&&!A.F(a.altKey)){r=A.e("helpDlg",t.m)
if(!A.F(r.open))r.showModal()}if($.b1!=null){s=A.q(a.target)
if(s==null)s=null
else s=A.iL(s,p)
s=s!==!0}else s=!1
if(s)A.nT(a)
if($.bs!=null){s=A.q(a.target)
if(s==null)s=null
else s=A.iL(s,p)
s=s!==!0}else s=!1
if(s)if((A.F(a.ctrlKey)||A.F(a.metaKey))&&A.B(a.key).toLowerCase()==="z"){a.preventDefault()
A.l7()}else if(!A.F(a.ctrlKey)&&!A.F(a.metaKey)&&!A.F(a.altKey)){s=t.N
q=A.df(["p","pencil","f","fill","i","pick"],s,s).v(0,A.B(a.key).toLowerCase())
if(q!=null)A.cU(q,!0)}},
$S:1}
A.hd.prototype={
$1(a){A.c(a)
return A.au()},
$S:2}
A.he.prototype={
$1(a){A.c(a)
return A.au()},
$S:2}
A.fJ.prototype={
$1(a){var s,r
A.c(a)
s=$.aC()
r=this.b
return this.a===s?A.kI(s,r):A.kX(r)},
$S:2}
A.fK.prototype={
$1(a){var s,r,q,p,o,n,m,l,k,j,i,h,g,f
A.c(a)
s=this.a
r=this.b
a.preventDefault()
$.cR=new A.i(s,r)
q=t.m
p=A.e("ctxPaste",q)
o=s!==$.t()
p.hidden=o
A.e("ctxSep",q).hidden=o
n=A.e("ctxColors",q)
n.hidden=o
m=s.b.b?B.e:B.d
if(!(r<m.length))return A.a(m,r)
n.textContent="Edit "+A.bJ(m[r])+" colors\u2026"
m=A.e("ctxLogo",q)
m.hidden=o
n=s.b.b?B.e:B.d
if(!(r<n.length))return A.a(n,r)
m.textContent="Edit "+A.bJ(n[r])+" logo\u2026"
p.disabled=$.bP==null
n=$.bP
n=n==null?"Paste helmet here":"Paste "+A.bJ(n.a)+" helmet here"
p.textContent=n
n=A.e("ctxCopy",q)
s=s.b.b?B.e:B.d
if(!(r<s.length))return A.a(s,r)
n.textContent="Copy "+A.bJ(s[r])+" helmet"
l=A.e("ctx",q)
l.hidden=!1
k=A.j(l.offsetWidth)
j=A.j(l.offsetHeight)
i=A.a_(a.clientX)
h=A.a_(a.clientY)
s=v.G
g=i+k>A.j(A.c(s.window).innerWidth)?i-k:i
f=h+j>A.j(A.c(s.window).innerHeight)?h-j:h
s=A.c(l.style)
s.left=A.k(g<0?0:g)+"px"
s.top=A.k(f<0?0:f)+"px"
return null},
$S:2}
A.ip.prototype={
$1(a){A.c(a)
$.cM=this.a
A.io()},
$S:1}
A.iq.prototype={
$1(a){A.c(a)
B.b.q($.b_,$.cM,this.a)
A.io()},
$S:1}
A.fC.prototype={
$1(a){return A.F(a)},
$S:44}
A.hN.prototype={
$1(a){var s,r,q,p,o,n,m,l
A.c(a)
s=A.je(a,!1)
if($.b1==null||s==null)return
a.preventDefault()
r=A.j(a.button)===2
q=$.e3
if(q==="pick"&&!r){q=$.a9()
p=s.b*72+s.a
if(!(p>=0&&p<q.length))return A.a(q,p)
o=q[p]
if(o!==0)$.ao=o
A.cV("pencil")
A.iv()
return}if(q==="move"){q=$.an
if(q==null){q=$.jn
q=q==null?null:q.e
n=q}else n=q
if(n==null)n=A.d([],t.n)
m=B.b.bs(n,new A.hM(s))
$.aA=m<0?null:m
if(m>=0){A.bq()
q=new Uint8Array(A.z($.a9()))
p=A.J(n,t.R)
$.dY=new A.cx([p,s,m,q])
this.a.setPointerCapture(A.j(a.pointerId))}A.bQ()
return}A.bq()
if($.e3==="fill"){A.nA(s,r?0:$.ao)
A.az()
return}$.e4=!0
$.jr=r?0:$.ao
$.jm=s
this.a.setPointerCapture(A.j(a.pointerId))
q=$.a9()
p=s.b*72+s.a
l=$.jr
q.$flags&2&&A.r(q)
if(!(p>=0&&p<q.length))return A.a(q,p)
q[p]=l
A.bQ()},
$S:1}
A.hM.prototype={
$1(a){var s,r,q,p
t.R.a(a)
s=this.a
r=s.a
q=a.a
p=!1
if(r>=q)if(r<q+8){s=s.b
r=a.b
s=s>=r&&s<r+8}else s=p
else s=p
return s},
$S:6}
A.hO.prototype={
$1(a){var s,r,q,p,o,n,m,l
A.c(a)
s=$.dY
if(s!=null){r=A.je(a,!0)
q=s.a
p=q[3]
o=q[0]
n=q[2]
m=r.a
q=q[1]
l=A.kT(p,o,n,m-q.a,r.b-q.b)
$.cQ=l.b
$.an=l.a
$.aA=0
A.bQ()
return}if(!$.e4)return
q=A.je(a,!0)
q.toString
p=$.jm
p.toString
A.oe(p,q)
$.jm=q
A.bQ()},
$S:1}
A.i0.prototype={
$1(a){A.c(a)
if(!$.e4&&$.dY==null)return
$.e4=!1
$.dY=null
A.az()},
$S:2}
A.hP.prototype={
$1(a){return A.c(a).preventDefault()},
$S:2}
A.hT.prototype={
$0(){return A.cV("pencil")},
$S:0}
A.hU.prototype={
$0(){return A.cV("fill")},
$S:0}
A.hV.prototype={
$0(){return A.cV("pick")},
$S:0}
A.hW.prototype={
$0(){return A.cV("move")},
$S:0}
A.hX.prototype={
$0(){$.e2=!$.e2
A.iv()},
$S:0}
A.hY.prototype={
$0(){A.bq()
$.aA=$.an=null
A.az()},
$S:0}
A.hZ.prototype={
$1(a){A.c(a)
return A.bQ()},
$S:2}
A.i_.prototype={
$0(){A.bq()
var s=$.a9()
B.h.a7(s,0,s.length,0)
A.az()},
$S:0}
A.hQ.prototype={
$0(){A.bq()
$.cQ=new Uint8Array(A.z($.iF()))
$.aA=$.an=null
var s=A.J($.h2,t.S)
$.a0=s
$.e1=null
A.az()},
$S:0}
A.hR.prototype={
$0(){return this.a.click()},
$S:0}
A.hS.prototype={
$1(a){var s,r,q
A.c(a)
s=this.a
r=A.q(s.files)
q=r==null?null:A.q(r.item(0))
if(q!=null)A.fW(q)
s.value=""},
$S:1}
A.iw.prototype={
$1(a){A.c(a)
$.ao=this.a
A.iv()},
$S:1}
A.ix.prototype={
$1(a){var s,r,q
A.c(a)
s=$.a0
r=$.ao-1
if(!(r>=0&&r<s.length))return A.a(s,r)
q=this.a
if(s[r]===q)return
A.bq()
B.b.q($.a0,$.ao-1,q)
A.az()},
$S:1}
A.fB.prototype={
$1(a){var s,r=this.a
A.j(a)
if(!(a>=0&&a<r.length))return A.a(r,a)
r=r[a]
s=this.b
if(!(a<s.length))return A.a(s,a)
return r.N(0,s[a])},
$S:45}
A.fL.prototype={
$1(a){var s,r
A.c(a)
s=$.aC()
r=this.b
return this.a===s?A.kJ(s,r):A.kY(r)},
$S:2}
A.fM.prototype={
$1(a){var s,r,q,p,o,n,m,l,k,j,i,h,g
A.c(a)
s=this.a
r=this.b
a.preventDefault()
A.au()
$.e7=new A.i(s,r)
q=s.b.b?B.e:B.d
if(!(r<q.length))return A.a(q,r)
p=A.bJ(q[r])
r=t.m
A.e("ctxSCopy",r).textContent="Copy "+p+" small helmet"
o=A.e("ctxSPaste",r)
n=s!==$.t()
o.hidden=n
o.disabled=$.e6==null
s=$.e6
s=s==null?"Paste small helmet here":"Paste "+A.bJ(s.a)+" small helmet here"
o.textContent=s
A.e("ctxSSep",r).hidden=n
s=A.e("ctxSEdit",r)
s.hidden=n
s.textContent="Edit "+p+" small helmet\u2026"
o.hidden=n
m=A.e("ctxS",r)
m.hidden=!1
l=A.j(m.offsetWidth)
k=A.j(m.offsetHeight)
j=A.a_(a.clientX)
i=A.a_(a.clientY)
s=v.G
h=j+l>A.j(A.c(s.window).innerWidth)?j-l:j
g=i+k>A.j(A.c(s.window).innerHeight)?i-k:i
s=A.c(m.style)
s.left=A.k(h<0?0:h)+"px"
s.top=A.k(g<0?0:g)+"px"
return null},
$S:2}
A.i1.prototype={
$0(){var s=$.e7
A.au()
if(s!=null)A.kJ(s.a,s.b)},
$S:0}
A.i2.prototype={
$0(){var s=$.e7
A.au()
if(s!=null)A.kY(s.b)},
$S:0}
A.i3.prototype={
$0(){var s=$.e7
A.au()
if(s!=null)A.kV(s.b,null)},
$S:0}
A.i8.prototype={
$1(a){var s,r,q,p,o,n,m
A.c(a)
s=A.l5(a,!1)
if($.L==null||s==null)return
a.preventDefault()
r=A.j(a.button)===2
q=$.a2==="bg"
p=q?16:24
if($.iA==="pick"&&!r){o=$.L
q=q?o.ga5():o.T().a
o=s.b*p+s.a
if(!(o>=0&&o<q.length))return A.a(q,o)
n=q[o]
if(n!==0)$.e5=n
A.cU("pencil",!0)
A.aB()
return}q=s.a
o=s.b
if(!A.on(q,o))return
A.hx()
if($.iA==="fill"){A.oo(s,r?0:$.e5)
A.aB()
return}$.e8=!0
$.jz=r?0:$.e5
$.jy=s
try{this.a.setPointerCapture(A.j(a.pointerId))}catch(m){}A.oq(q,o,$.jz)
A.dZ()},
$S:1}
A.i9.prototype={
$1(a){var s,r
A.c(a)
if(!$.e8)return
s=A.l5(a,!0)
s.toString
r=$.jy
r.toString
A.or(r,s)
$.jy=s
A.dZ()},
$S:1}
A.ih.prototype={
$1(a){A.c(a)
if(!$.e8)return
$.e8=!1
A.aB()},
$S:2}
A.ia.prototype={
$1(a){return A.c(a).preventDefault()},
$S:2}
A.ib.prototype={
$0(){return A.ju("bg",!0)},
$S:0}
A.ic.prototype={
$0(){return A.ju("overlay",!0)},
$S:0}
A.id.prototype={
$0(){return A.cU("pencil",!0)},
$S:0}
A.ie.prototype={
$0(){return A.cU("fill",!0)},
$S:0}
A.ig.prototype={
$0(){return A.cU("pick",!0)},
$S:0}
A.i4.prototype={
$1(a){A.c(a)
return A.dZ()},
$S:2}
A.i5.prototype={
$0(){var s=$.L
if(s==null)return
A.hx()
s.bw(new A.i(s.w,s.d.c))
$.br=A.d([],t.s)
A.aB()},
$S:0}
A.i6.prototype={
$0(){return this.a.click()},
$S:0}
A.i7.prototype={
$1(a){var s,r,q
A.c(a)
s=this.a
r=A.q(s.files)
q=r==null?null:A.q(r.item(0))
if(q!=null)A.fX(q)
s.value=""},
$S:1}
A.is.prototype={
$1(a){A.c(a)
$.e5=this.a
A.l4()},
$S:1}
A.it.prototype={
$1(a){var s
A.j(a)
s=this.a
if(0>=s.length)return A.a(s,0)
return a===s[0]},
$S:3}
A.iu.prototype={
$1(a){var s,r,q
A.c(a)
s=this.a
r=t.S
q=this.b
if(B.b.M(A.eh(s.gY(),r),new A.ir(q)))return
A.hx()
s.x=t.L.a(A.aU(4,q,!1,r))
A.aB()},
$S:1}
A.ir.prototype={
$1(a){return A.j(a)===this.a},
$S:3}
A.ii.prototype={
$1(a){return t.Q.a(a).a===this.a},
$S:17}
A.ij.prototype={
$1(a){A.c(a)
return A.dU()},
$S:2}
A.ik.prototype={
$1(a){A.c(a)
return A.dU()},
$S:2}
A.il.prototype={
$1(a){A.c(a)
return A.dU()},
$S:2}
A.im.prototype={
$1(a){A.c(a)
return A.dU()},
$S:2}
A.fH.prototype={
$1(a){return t.Q.a(a).a===this.a},
$S:17}
A.hJ.prototype={
$0(){return this.a.showModal()},
$S:0}
A.hK.prototype={
$0(){return this.a.close()},
$S:0}
A.hL.prototype={
$1(a){var s=this.a
if(A.q(A.c(a).target)===s)s.close()},
$S:1};(function aliases(){var s=J.aT.prototype
s.bE=s.j
s=A.v.prototype
s.b2=s.aZ})();(function installTearOffs(){var s=hunkHelpers._static_2,r=hunkHelpers._static_1,q=hunkHelpers._static_0
s(J,"mJ","jM",47)
r(A,"nl","m1",7)
r(A,"nm","m2",7)
r(A,"nn","m3",7)
q(A,"kF","nb",0)
r(A,"kK","m7",4)
q(A,"nZ","nj",0)
q(A,"nX","nh",0)
q(A,"o1","bK",0)
q(A,"o8","oA",0)
q(A,"o3","bM",0)
q(A,"o7","oh",0)
r(A,"o0","bJ",32)
q(A,"o2","bL",0)
q(A,"o9","l6",0)
q(A,"nY","ni",0)
q(A,"o5","nw",0)
q(A,"o4","bN",0)
q(A,"oa","l7",0)
q(A,"o_","nk",0)
q(A,"o6","nx",0)})();(function inheritance(){var s=hunkHelpers.mixin,r=hunkHelpers.inherit,q=hunkHelpers.inheritMany
r(A.x,null)
q(A.x,[A.iN,J.d9,A.cj,J.bV,A.bB,A.D,A.et,A.h,A.ad,A.cc,A.bf,A.c0,A.co,A.G,A.ag,A.cr,A.aW,A.eI,A.eo,A.c1,A.cA,A.aR,A.ca,A.ef,A.b8,A.aG,A.b7,A.c5,A.dL,A.dE,A.dy,A.dP,A.f8,A.as,A.dJ,A.f6,A.f4,A.dF,A.ak,A.dH,A.bg,A.T,A.dG,A.dN,A.cG,A.dK,A.aO,A.v,A.d6,A.eP,A.f9,A.eQ,A.dq,A.ck,A.eR,A.c2,A.W,A.Y,A.dQ,A.dx,A.en,A.d7,A.ax,A.bh,A.aa,A.ee,A.a1,A.ej,A.c9,A.a5,A.ei,A.em,A.dv,A.eu,A.bc,A.eD,A.er,A.aM,A.ev,A.eF,A.p,A.ep])
q(J.d9,[J.db,J.c4,J.c6,J.bw,J.bx,J.bv,J.aS])
q(J.c6,[J.aT,J.u,A.aV,A.ce])
q(J.aT,[J.dr,J.bd,J.aF])
r(J.da,A.cj)
r(J.ed,J.u)
q(J.bv,[J.c3,J.dc])
q(A.D,[A.de,A.aJ,A.dd,A.dC,A.du,A.dI,A.d0,A.ap,A.cn,A.dB,A.cl,A.d5])
q(A.h,[A.bY,A.cb,A.aL,A.bA,A.dD,A.dO])
q(A.bY,[A.O,A.c_,A.c7,A.c8,A.N])
q(A.O,[A.cm,A.ae,A.aH,A.cq])
r(A.bZ,A.cb)
q(A.ag,[A.bj,A.bD,A.bk])
q(A.bj,[A.i,A.cw])
r(A.bE,A.bD)
q(A.bk,[A.cx,A.cy])
q(A.aW,[A.bW,A.cz])
r(A.bX,A.bW)
r(A.cf,A.aJ)
q(A.aR,[A.d3,A.d4,A.dz,A.fY,A.h_,A.eM,A.eL,A.fc,A.f_,A.hv,A.hw,A.iE,A.fu,A.fv,A.fw,A.fx,A.fy,A.fh,A.hI,A.fN,A.fO,A.hq,A.fk,A.fp,A.fm,A.fq,A.fr,A.fn,A.fi,A.fe,A.ff,A.hD,A.hC,A.hy,A.eE,A.ez,A.eC,A.ey,A.iz,A.hF,A.fU,A.ho,A.fj,A.fg,A.iB,A.ht,A.h5,A.hf,A.hg,A.hh,A.hi,A.hj,A.hk,A.hm,A.hb,A.hc,A.hd,A.he,A.fJ,A.fK,A.ip,A.iq,A.fC,A.hN,A.hM,A.hO,A.i0,A.hP,A.hZ,A.hS,A.iw,A.ix,A.fB,A.fL,A.fM,A.i8,A.i9,A.ih,A.ia,A.i4,A.i7,A.is,A.it,A.iu,A.ir,A.ii,A.ij,A.ik,A.il,A.im,A.fH,A.hL])
q(A.dz,[A.dw,A.bu])
r(A.b6,A.ca)
q(A.d4,[A.fZ,A.fd,A.fG,A.f0,A.ek,A.hu,A.fz,A.hp,A.hr,A.fo,A.h1,A.hA,A.hH,A.hG,A.hE,A.iC,A.iD])
r(A.bz,A.aV)
q(A.ce,[A.dh,A.X])
q(A.X,[A.cs,A.cu])
r(A.ct,A.cs)
r(A.cd,A.ct)
r(A.cv,A.cu)
r(A.af,A.cv)
q(A.cd,[A.di,A.dj])
q(A.af,[A.dk,A.dl,A.dm,A.dn,A.dp,A.b9,A.ba])
r(A.cB,A.dI)
q(A.d3,[A.eN,A.eO,A.f5,A.eS,A.eW,A.eV,A.eU,A.eT,A.eZ,A.eY,A.eX,A.f3,A.fA,A.ft,A.fl,A.fs,A.fF,A.fE,A.hB,A.hz,A.eA,A.eB,A.ew,A.ex,A.h3,A.h4,A.hl,A.h6,A.h7,A.h8,A.h9,A.ha,A.hT,A.hU,A.hV,A.hW,A.hX,A.hY,A.i_,A.hQ,A.hR,A.i1,A.i2,A.i3,A.ib,A.ic,A.id,A.ie,A.ig,A.i5,A.i6,A.hJ,A.hK])
r(A.cp,A.dH)
r(A.dM,A.cG)
r(A.aN,A.cz)
q(A.d6,[A.ea,A.eK])
q(A.ap,[A.cg,A.d8])
r(A.dA,A.eQ)
s(A.cs,A.v)
s(A.ct,A.G)
s(A.cu,A.v)
s(A.cv,A.G)})()
var v={G:typeof self!="undefined"?self:globalThis,typeUniverse:{eC:new Map(),tR:{},eT:{},tPV:{},sEA:[]},mangledGlobalNames:{b:"int",w:"double",a7:"num",P:"String",m:"bool",Y:"Null",l:"List",x:"Object",dg:"Map",E:"JSObject"},mangledNames:{},types:["~()","Y(E)","~(E)","m(b)","b(b)","m(b,b)","m(+(b,b))","~(~())","~(@)","~(b,b)","~(b,b,b)","al()","Y(@)","Y()","b()","ay<b>()","~(d2,b)","m(p)","m(P)","@(P)","ax(l<a5>)","c9(l<a5>,ax,m)","~(b,@)","Y(x,aX)","aa?()","m(aa)","l<al>({keep!m})","~(x?,x?)","b(a5)","b(aa)","+col,fineX,fineY,key,row(b,b,b,b,b)()","@(@,P)","P(P)","b?()","W<b,b>(W<b,b>,W<b,b>)","b(aM)","m(aM)","Y(~())","b(b,b)","b(b,b,b)","l<b>()","~(d2)","b(+(b,b))","@(@)","m(m)","m(@)","P(b)","b(@,@)","m(W<b,bh>)","Y(@,aX)"],interceptorsByTag:null,leafTags:null,arrayRti:Symbol("$ti"),rttc:{"2;":(a,b)=>c=>c instanceof A.i&&a.b(c.a)&&b.b(c.b),"2;boxes,pixels":(a,b)=>c=>c instanceof A.cw&&a.b(c.a)&&b.b(c.b),"3;":(a,b,c)=>d=>d instanceof A.bE&&a.b(d.a)&&b.b(d.b)&&c.b(d.c),"4;boxes,from,index,pixels":a=>b=>b instanceof A.cx&&A.kW(a,b.a),"5;col,fineX,fineY,key,row":a=>b=>b instanceof A.cy&&A.kW(a,b.a)}}
A.mm(v.typeUniverse,JSON.parse('{"aF":"aT","dr":"aT","bd":"aT","oE":"aV","db":{"m":[],"y":[]},"c4":{"y":[]},"c6":{"E":[]},"aT":{"E":[]},"u":{"l":["1"],"E":[],"h":["1"],"V":["1"]},"da":{"cj":[]},"ed":{"u":["1"],"l":["1"],"E":[],"h":["1"],"V":["1"]},"bV":{"H":["1"]},"bv":{"w":[],"a7":[],"aD":["a7"]},"c3":{"w":[],"b":[],"a7":[],"aD":["a7"],"y":[]},"dc":{"w":[],"a7":[],"aD":["a7"],"y":[]},"aS":{"P":[],"aD":["P"],"eq":[],"V":["@"],"y":[]},"bB":{"d2":[]},"de":{"D":[]},"bY":{"h":["1"]},"O":{"h":["1"]},"cm":{"O":["1"],"h":["1"],"h.E":"1","O.E":"1"},"ad":{"H":["1"]},"cb":{"h":["2"],"h.E":"2"},"bZ":{"cb":["1","2"],"h":["2"],"h.E":"2"},"cc":{"H":["2"]},"ae":{"O":["2"],"h":["2"],"h.E":"2","O.E":"2"},"aL":{"h":["1"],"h.E":"1"},"bf":{"H":["1"]},"c_":{"h":["1"],"h.E":"1"},"c0":{"H":["1"]},"bA":{"h":["1"],"h.E":"1"},"co":{"H":["1"]},"aH":{"O":["1"],"h":["1"],"h.E":"1","O.E":"1"},"i":{"bj":[],"ag":[]},"cw":{"bj":[],"ag":[]},"bE":{"bD":[],"ag":[]},"cx":{"bk":[],"ag":[]},"cy":{"bk":[],"ag":[]},"cr":{"H":["1"]},"bW":{"aW":["1"],"ay":["1"],"h":["1"]},"bX":{"bW":["1"],"aW":["1"],"ay":["1"],"h":["1"]},"cf":{"aJ":[],"D":[]},"dd":{"D":[]},"dC":{"D":[]},"cA":{"aX":[]},"aR":{"b4":[]},"d3":{"b4":[]},"d4":{"b4":[]},"dz":{"b4":[]},"dw":{"b4":[]},"bu":{"b4":[]},"du":{"D":[]},"b6":{"ca":["1","2"],"jP":["1","2"],"dg":["1","2"]},"c7":{"h":["1"],"h.E":"1"},"b8":{"H":["1"]},"c8":{"h":["1"],"h.E":"1"},"aG":{"H":["1"]},"N":{"h":["W<1,2>"],"h.E":"W<1,2>"},"b7":{"H":["W<1,2>"]},"bj":{"ag":[]},"bD":{"ag":[]},"bk":{"ag":[]},"c5":{"lS":[],"eq":[]},"dL":{"ci":[],"by":[]},"dD":{"h":["ci"],"h.E":"ci"},"dE":{"H":["ci"]},"dy":{"by":[]},"dO":{"h":["by"],"h.E":"by"},"dP":{"H":["by"]},"bz":{"aV":[],"E":[],"y":[]},"b9":{"af":[],"v":["b"],"X":["b"],"l":["b"],"ab":["b"],"E":[],"V":["b"],"h":["b"],"G":["b"],"y":[],"v.E":"b","G.E":"b"},"ba":{"af":[],"al":[],"v":["b"],"X":["b"],"l":["b"],"ab":["b"],"E":[],"V":["b"],"h":["b"],"G":["b"],"y":[],"v.E":"b","G.E":"b"},"aV":{"E":[],"y":[]},"ce":{"E":[]},"dh":{"E":[],"y":[]},"X":{"ab":["1"],"E":[],"V":["1"]},"cd":{"v":["w"],"X":["w"],"l":["w"],"ab":["w"],"E":[],"V":["w"],"h":["w"],"G":["w"]},"af":{"v":["b"],"X":["b"],"l":["b"],"ab":["b"],"E":[],"V":["b"],"h":["b"],"G":["b"]},"di":{"v":["w"],"X":["w"],"l":["w"],"ab":["w"],"E":[],"V":["w"],"h":["w"],"G":["w"],"y":[],"v.E":"w","G.E":"w"},"dj":{"v":["w"],"X":["w"],"l":["w"],"ab":["w"],"E":[],"V":["w"],"h":["w"],"G":["w"],"y":[],"v.E":"w","G.E":"w"},"dk":{"af":[],"v":["b"],"X":["b"],"l":["b"],"ab":["b"],"E":[],"V":["b"],"h":["b"],"G":["b"],"y":[],"v.E":"b","G.E":"b"},"dl":{"af":[],"v":["b"],"X":["b"],"l":["b"],"ab":["b"],"E":[],"V":["b"],"h":["b"],"G":["b"],"y":[],"v.E":"b","G.E":"b"},"dm":{"af":[],"v":["b"],"X":["b"],"l":["b"],"ab":["b"],"E":[],"V":["b"],"h":["b"],"G":["b"],"y":[],"v.E":"b","G.E":"b"},"dn":{"af":[],"v":["b"],"X":["b"],"l":["b"],"ab":["b"],"E":[],"V":["b"],"h":["b"],"G":["b"],"y":[],"v.E":"b","G.E":"b"},"dp":{"af":[],"v":["b"],"X":["b"],"l":["b"],"ab":["b"],"E":[],"V":["b"],"h":["b"],"G":["b"],"y":[],"v.E":"b","G.E":"b"},"dI":{"D":[]},"cB":{"aJ":[],"D":[]},"ak":{"D":[]},"cp":{"dH":["1"]},"T":{"b5":["1"]},"cG":{"k3":[]},"dM":{"cG":[],"k3":[]},"aN":{"aW":["1"],"jQ":["1"],"ay":["1"],"h":["1"]},"aO":{"H":["1"]},"ca":{"dg":["1","2"]},"aW":{"ay":["1"],"h":["1"]},"cz":{"aW":["1"],"ay":["1"],"h":["1"]},"w":{"a7":[],"aD":["a7"]},"b":{"a7":[],"aD":["a7"]},"l":{"h":["1"]},"a7":{"aD":["a7"]},"ci":{"by":[]},"ay":{"h":["1"]},"P":{"aD":["P"],"eq":[]},"d0":{"D":[]},"aJ":{"D":[]},"ap":{"D":[]},"cg":{"D":[]},"d8":{"D":[]},"cn":{"D":[]},"dB":{"D":[]},"cl":{"D":[]},"d5":{"D":[]},"dq":{"D":[]},"ck":{"D":[]},"cq":{"O":["1"],"h":["1"],"h.E":"1","O.E":"1"},"dQ":{"aX":[]},"lG":{"l":["b"],"h":["b"]},"al":{"l":["b"],"h":["b"]},"m_":{"l":["b"],"h":["b"]},"lE":{"l":["b"],"h":["b"]},"lY":{"l":["b"],"h":["b"]},"lF":{"l":["b"],"h":["b"]},"lZ":{"l":["b"],"h":["b"]},"lC":{"l":["w"],"h":["w"]},"lD":{"l":["w"],"h":["w"]}}'))
A.ml(v.typeUniverse,JSON.parse('{"bY":1,"X":1,"cz":1,"d6":2}'))
var u={c:"Error handler must accept one Object or one Object and a StackTrace as arguments, and return a value of the returned future's type",b:"The picture has no transparency, so its corner color was used as the background."}
var t=(function rtii(){var s=A.aP
return{u:s("ak"),h:s("aD<@>"),C:s("D"),Z:s("b4"),I:s("aa"),k:s("h<w>"),r:s("h<@>"),D:s("h<b>"),U:s("u<aa>"),q:s("u<l<a5>>"),G:s("u<ba>"),n:s("u<+(b,b)>"),bO:s("u<+col,fineX,fineY,key,row(b,b,b,b,b)>"),W:s("u<dv>"),s:s("u<P>"),dc:s("u<al>"),A:s("u<aM>"),Y:s("u<a5>"),b:s("u<@>"),t:s("u<b>"),cj:s("u<aa?>"),a:s("u<b?>"),e:s("V<@>"),T:s("c4"),m:s("E"),g:s("aF"),da:s("ab<@>"),w:s("l<a5>"),j:s("l<@>"),L:s("l<b>"),cz:s("W<b,bh>"),aS:s("W<b,b>"),af:s("bz"),E:s("af"),c:s("b9"),P:s("Y"),K:s("x"),be:s("ax"),cY:s("oF"),cD:s("+()"),J:s("+(dg<b,al>,l<b>)"),R:s("+(b,b)"),a0:s("ci"),f:s("ay<b>"),l:s("aX"),N:s("P"),Q:s("p"),bW:s("y"),b7:s("aJ"),p:s("al"),cr:s("bd"),aA:s("aL<p>"),d:s("bA<b>"),B:s("aM"),_:s("T<@>"),x:s("bh"),bY:s("a5"),y:s("m"),bG:s("m(x)"),ax:s("m(p)"),i:s("w"),z:s("@"),bd:s("@()"),v:s("@(x)"),V:s("@(x,aX)"),S:s("b"),bc:s("b5<Y>?"),aQ:s("E?"),cM:s("a1?"),X:s("x?"),aD:s("P?"),F:s("bg<@,@>?"),O:s("dK?"),cG:s("m?"),dd:s("w?"),a3:s("b?"),ae:s("a7?"),o:s("a7"),H:s("~"),M:s("~()")}})();(function constants(){var s=hunkHelpers.makeConstList
B.K=J.d9.prototype
B.b=J.u.prototype
B.a=J.c3.prototype
B.m=J.bv.prototype
B.c=J.aS.prototype
B.L=J.aF.prototype
B.M=J.c6.prototype
B.h=A.ba.prototype
B.x=J.dr.prototype
B.u=J.bd.prototype
B.y=new A.ea()
B.z=new A.c0(A.aP("c0<0&>"))
B.v=function getTagFallback(o) {
  var s = Object.prototype.toString.call(o);
  return s.substring(8, s.length - 1);
}
B.A=function() {
  var toStringFunction = Object.prototype.toString;
  function getTag(o) {
    var s = toStringFunction.call(o);
    return s.substring(8, s.length - 1);
  }
  function getUnknownTag(object, tag) {
    if (/^HTML[A-Z].*Element$/.test(tag)) {
      var name = toStringFunction.call(object);
      if (name == "[object Object]") return null;
      return "HTMLElement";
    }
  }
  function getUnknownTagGenericBrowser(object, tag) {
    if (object instanceof HTMLElement) return "HTMLElement";
    return getUnknownTag(object, tag);
  }
  function prototypeForTag(tag) {
    if (typeof window == "undefined") return null;
    if (typeof window[tag] == "undefined") return null;
    var constructor = window[tag];
    if (typeof constructor != "function") return null;
    return constructor.prototype;
  }
  function discriminator(tag) { return null; }
  var isBrowser = typeof HTMLElement == "function";
  return {
    getTag: getTag,
    getUnknownTag: isBrowser ? getUnknownTagGenericBrowser : getUnknownTag,
    prototypeForTag: prototypeForTag,
    discriminator: discriminator };
}
B.F=function(getTagFallback) {
  return function(hooks) {
    if (typeof navigator != "object") return hooks;
    var userAgent = navigator.userAgent;
    if (typeof userAgent != "string") return hooks;
    if (userAgent.indexOf("DumpRenderTree") >= 0) return hooks;
    if (userAgent.indexOf("Chrome") >= 0) {
      function confirm(p) {
        return typeof window == "object" && window[p] && window[p].name == p;
      }
      if (confirm("Window") && confirm("HTMLElement")) return hooks;
    }
    hooks.getTag = getTagFallback;
  };
}
B.B=function(hooks) {
  if (typeof dartExperimentalFixupGetTag != "function") return hooks;
  hooks.getTag = dartExperimentalFixupGetTag(hooks.getTag);
}
B.E=function(hooks) {
  if (typeof navigator != "object") return hooks;
  var userAgent = navigator.userAgent;
  if (typeof userAgent != "string") return hooks;
  if (userAgent.indexOf("Firefox") == -1) return hooks;
  var getTag = hooks.getTag;
  var quickMap = {
    "BeforeUnloadEvent": "Event",
    "DataTransfer": "Clipboard",
    "GeoGeolocation": "Geolocation",
    "Location": "!Location",
    "WorkerMessageEvent": "MessageEvent",
    "XMLDocument": "!Document"};
  function getTagFirefox(o) {
    var tag = getTag(o);
    return quickMap[tag] || tag;
  }
  hooks.getTag = getTagFirefox;
}
B.D=function(hooks) {
  if (typeof navigator != "object") return hooks;
  var userAgent = navigator.userAgent;
  if (typeof userAgent != "string") return hooks;
  if (userAgent.indexOf("Trident/") == -1) return hooks;
  var getTag = hooks.getTag;
  var quickMap = {
    "BeforeUnloadEvent": "Event",
    "DataTransfer": "Clipboard",
    "HTMLDDElement": "HTMLElement",
    "HTMLDTElement": "HTMLElement",
    "HTMLPhraseElement": "HTMLElement",
    "Position": "Geoposition"
  };
  function getTagIE(o) {
    var tag = getTag(o);
    var newTag = quickMap[tag];
    if (newTag) return newTag;
    if (tag == "Object") {
      if (window.DataView && (o instanceof window.DataView)) return "DataView";
    }
    return tag;
  }
  function prototypeForTagIE(tag) {
    var constructor = window[tag];
    if (constructor == null) return null;
    return constructor.prototype;
  }
  hooks.getTag = getTagIE;
  hooks.prototypeForTag = prototypeForTagIE;
}
B.C=function(hooks) {
  var getTag = hooks.getTag;
  var prototypeForTag = hooks.prototypeForTag;
  function getTagFixed(o) {
    var tag = getTag(o);
    if (tag == "Document") {
      if (!!o.xmlVersion) return "!Document";
      return "!HTMLDocument";
    }
    return tag;
  }
  function prototypeForTagFixed(tag) {
    if (tag == "Document") return null;
    return prototypeForTag(tag);
  }
  hooks.getTag = getTagFixed;
  hooks.prototypeForTag = prototypeForTagFixed;
}
B.w=function(hooks) { return hooks; }

B.G=new A.dq()
B.l=new A.et()
B.H=new A.eK()
B.i=new A.dM()
B.n=new A.dQ()
B.I=new A.c2("Not an iNES ROM (missing NES<1A> header)",null,null)
B.J=new A.c2("ROM file is shorter than its header says",null,null)
B.N=s([112,115],t.t)
B.j=s([15,29,122,123],t.t)
B.k=s([24,25,122,123],t.t)
B.O=s([24,25,42,43],t.t)
B.d=s(["bills","colts","dolphins","patriots","jets","bengals","browns","oilers","steelers","broncos","chiefs","raiders","chargers","seahawks","redskins","giants","eagles","cardinals","cowboys","bears","lions","packers","vikings","buccaneers","49ers","rams","saints","falcons"],t.s)
B.e=s(["bills","dolphins","patriots","jets","bengals","browns","ravens","steelers","colts","texans","jaguars","titans","broncos","chiefs","raiders","chargers","redskins","giants","eagles","cowboys","bears","lions","packers","vikings","buccaneers","saints","falcons","panthers","49ers","rams","seahawks","cardinals"],t.s)
B.P=s([13,14,15,26,27,29,30,31],t.t)
B.Q=s([15,37,0,48,15,0,0,41,15,0,0,0,15,0,0,0,15,0,0,48,15,0,0,37,15,15,0,0,15,0,0,0],t.t)
B.R=s([],t.W)
B.S=s([],t.s)
B.T=s([],t.t)
B.o=s(["Shell 1","Shell 2","Shell 3","Facemask 1","Facemask 2","Logo 1","Logo 2","Logo 3"],t.s)
B.f=s([8355711,255,191,4664255,9896071,11206691,11211520,9115392,5451776,30720,27392,23296,17240,0,0,0,12566463,30968,22776,7030783,14352589,15138907,16267264,15163155,11501312,47104,43776,43847,35723,0,0,0,16316664,4177919,9992440,7047423,16283896,16275608,16283736,16753479,16300032,12122136,6019927,5830808,60379,7895160,0,0,16777215,11003903,12105976,14203128,16300280,16492483,15782064,16769963,16505723,14219384,12122296,12122328,65535,16308472,0,0],t.t)
B.ar=new A.p("cardinals","Arizona Cardinals",6033443,16758290,1118481)
B.au=new A.p("falcons","Atlanta Falcons",1711393,10950960,16777215)
B.ae=new A.p("ravens","Baltimore Ravens",2365299,13148746,1118481)
B.aa=new A.p("bills","Buffalo Bills",13197,12979248,16777215)
B.a6=new A.p("panthers","Carolina Panthers",1711393,34250,16777215)
B.ay=new A.p("bears","Chicago Bears",726570,13121539,16777215)
B.az=new A.p("bengals","Cincinnati Bengals",1711393,16469780,16777215)
B.ab=new A.p("browns","Cleveland Browns",3218688,16727040,16777215)
B.ax=new A.p("cowboys","Dallas Cowboys",269890,10857647,1118481)
B.aq=new A.p("broncos","Denver Broncos",664387,16469780,16777215)
B.an=new A.p("lions","Detroit Lions",14947,11581372,1118481)
B.a9=new A.p("packers","Green Bay Packers",2111281,16758290,1118481)
B.ag=new A.p("texans","Houston Texans",204847,10950960,16777215)
B.af=new A.p("colts","Indianapolis Colts",11359,10660525,1118481)
B.at=new A.p("jaguars","Jacksonville Jaguars",20574,14131754,1118481)
B.a7=new A.p("chiefs","Kansas City Chiefs",7998236,16758812,1118481)
B.ao=new A.p("dolphins","Miami Dolphins",21601,16534530,16777215)
B.ak=new A.p("vikings","Minnesota Vikings",3873374,16762415,1118481)
B.ac=new A.p("patriots","New England Patriots",8772,12979248,16777215)
B.aB=new A.p("saints","New Orleans Saints",1711393,13876365,1118481)
B.am=new A.p("giants","New York Giants",729701,10950960,16777215)
B.aA=new A.p("jets","New York Jets",1201984,16777215,1201984)
B.ah=new A.p("raiders","Oakland Raiders",1711393,10857647,1118481)
B.aj=new A.p("eagles","Philadelphia Eagles",19540,10857647,1118481)
B.as=new A.p("steelers","Pittsburgh Steelers",1711393,16758290,1118481)
B.a8=new A.p("chargers","San Diego Chargers",10846,16761358,1118481)
B.a5=new A.p("49ers","San Francisco 49ers",7208960,11770205,1118481)
B.ai=new A.p("seahawks","Seattle Seahawks",8772,6929960,1118481)
B.av=new A.p("rams","St. Louis Rams",8772,11770205,1118481)
B.ap=new A.p("buccaneers","Tampa Bay Buccaneers",3420203,13961738,16777215)
B.al=new A.p("titans","Tennessee Titans",795456,4952795,16777215)
B.aw=new A.p("redskins","Washington Redskins",5903380,16758290,1118481)
B.q=s([B.ar,B.au,B.ae,B.aa,B.a6,B.ay,B.az,B.ab,B.ax,B.aq,B.an,B.a9,B.ag,B.af,B.at,B.a7,B.ao,B.ak,B.ac,B.aB,B.am,B.aA,B.ah,B.aj,B.as,B.a8,B.a5,B.ai,B.av,B.ap,B.al,B.aw],A.aP("u<p>"))
B.V=new A.i(0,0)
B.W=new A.i(0,8)
B.X=new A.i(8,0)
B.Y=new A.i(8,8)
B.Z=new A.i("sFill","fill")
B.a_=new A.i("sPencil","pencil")
B.a0=new A.i("sPick","pick")
B.a1=new A.i("tFill","fill")
B.a2=new A.i("tMove","move")
B.a3=new A.i("tPencil","pencil")
B.a4=new A.i("tPick","pick")
B.U={}
B.p=new A.bX(B.U,0,A.aP("bX<b>"))
B.ad=new A.p("classic","Classic",30968,16300032,1118481)
B.r=new A.dA(0,"modern")
B.t=new A.dA(1,"nes")
B.aC=A.av("oB")
B.aD=A.av("oC")
B.aE=A.av("lC")
B.aF=A.av("lD")
B.aG=A.av("lE")
B.aH=A.av("lF")
B.aI=A.av("lG")
B.aJ=A.av("x")
B.aK=A.av("lY")
B.aL=A.av("lZ")
B.aM=A.av("m_")
B.aN=A.av("al")})();(function staticFields(){$.f1=null
$.ai=A.d([],A.aP("u<x>"))
$.jW=null
$.jJ=null
$.jI=null
$.kP=null
$.kE=null
$.l0=null
$.fT=null
$.h0=null
$.jj=null
$.f2=A.d([],A.aP("u<l<x>?>"))
$.bG=null
$.cH=null
$.cI=null
$.j5=!1
$.K=B.i
$.bP=null
$.fR=null
$.ji=null
$.cR=null
$.cS=null
$.js=null
$.cL=A.a4(t.S)
$.cX=A.d([],A.aP("u<+(al,ay<b>,ay<b>)>"))
$.dX=null
$.b_=A.d([],t.t)
$.cM=0
$.jh=""
$.b1=null
$.an=null
$.kQ=A.d([],t.n)
$.aA=null
$.a0=A.d([],t.t)
$.h2=A.d([],t.t)
$.e2=!1
$.bp=A.d([],A.aP("u<+(al,l<+(b,b)>?,l<b>)>"))
$.dY=null
$.ao=1
$.e3="pencil"
$.jp=null
$.jn=null
$.e1=null
$.e4=!1
$.jr=1
$.jm=null
$.e6=null
$.iy=null
$.jx=null
$.jw=""
$.cW=A.a4(t.S)
$.e7=null
$.L=null
$.bs=null
$.a2="bg"
$.iA="pencil"
$.e5=1
$.jA=""
$.br=A.d([],t.s)
$.b3=A.d([],A.aP("u<+(dg<b,al>,l<b>)>"))
$.e8=!1
$.jz=1
$.jy=null})();(function lazyInitializers(){var s=hunkHelpers.lazyFinal,r=hunkHelpers.lazy
s($,"oD","a8",()=>A.nE("_$dart_dartClosure"))
s($,"oU","cY",()=>A.el(0))
s($,"oX","lm",()=>A.d([new J.da()],A.aP("u<cj>")))
s($,"oH","l9",()=>A.aK(A.eJ({
toString:function(){return"$receiver$"}})))
s($,"oI","la",()=>A.aK(A.eJ({$method$:null,
toString:function(){return"$receiver$"}})))
s($,"oJ","lb",()=>A.aK(A.eJ(null)))
s($,"oK","lc",()=>A.aK(function(){var $argumentsExpr$="$arguments$"
try{null.$method$($argumentsExpr$)}catch(q){return q.message}}()))
s($,"oN","lf",()=>A.aK(A.eJ(void 0)))
s($,"oO","lg",()=>A.aK(function(){var $argumentsExpr$="$arguments$"
try{(void 0).$method$($argumentsExpr$)}catch(q){return q.message}}()))
s($,"oM","le",()=>A.aK(A.k1(null)))
s($,"oL","ld",()=>A.aK(function(){try{null.$method$}catch(q){return q.message}}()))
s($,"oQ","li",()=>A.aK(A.k1(void 0)))
s($,"oP","lh",()=>A.aK(function(){try{(void 0).$method$}catch(q){return q.message}}()))
s($,"oR","jD",()=>A.m0())
s($,"oT","lk",()=>A.lP(A.z(A.d([-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-1,-2,-2,-2,-2,-2,62,-2,62,-2,63,52,53,54,55,56,57,58,59,60,61,-2,-2,-2,-1,-2,-2,-2,0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,-2,-2,-2,-2,63,-2,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40,41,42,43,44,45,46,47,48,49,50,51,-2,-2,-2,-2,-2],t.t))))
r($,"oS","lj",()=>A.el(0))
s($,"oW","e9",()=>A.kU(B.aJ))
s($,"oV","ll",()=>A.lO(256,new A.fg(),t.S))
s($,"p1","aC",()=>A.jV("src"))
s($,"oZ","t",()=>A.jV("dst"))
r($,"cQ","a9",()=>A.el(0))
r($,"nU","iF",()=>A.el(0))
s($,"p0","ln",()=>{var q,p=t.m
p=A.mu(A.nF(A.os(),"document",p),"createElement","canvas",p)
q=t.S
A.kB(p,"width",72,q)
A.kB(p,"height",72,q)
return p})})();(function nativeSupport(){!function(){var s=function(a){var m={}
m[a]=1
return Object.keys(hunkHelpers.convertToFastObject(m))[0]}
v.getIsolateTag=function(a){return s("___dart_"+a+v.isolateTag)}
var r="___dart_isolate_tags_"
var q=Object[r]||(Object[r]=Object.create(null))
var p="_ZxYxX"
for(var o=0;;o++){var n=s(p+"_"+o+"_")
if(!(n in q)){q[n]=1
v.isolateTag=n
break}}v.dispatchPropertyName=v.getIsolateTag("dispatch_record")}()
hunkHelpers.setOrUpdateInterceptorsByTag({SharedArrayBuffer:A.aV,ArrayBuffer:A.bz,ArrayBufferView:A.ce,DataView:A.dh,Float32Array:A.di,Float64Array:A.dj,Int16Array:A.dk,Int32Array:A.dl,Int8Array:A.dm,Uint16Array:A.dn,Uint32Array:A.dp,Uint8ClampedArray:A.b9,CanvasPixelArray:A.b9,Uint8Array:A.ba})
hunkHelpers.setOrUpdateLeafTags({SharedArrayBuffer:true,ArrayBuffer:true,ArrayBufferView:false,DataView:true,Float32Array:true,Float64Array:true,Int16Array:true,Int32Array:true,Int8Array:true,Uint16Array:true,Uint32Array:true,Uint8ClampedArray:true,CanvasPixelArray:true,Uint8Array:false})
A.X.$nativeSuperclassTag="ArrayBufferView"
A.cs.$nativeSuperclassTag="ArrayBufferView"
A.ct.$nativeSuperclassTag="ArrayBufferView"
A.cd.$nativeSuperclassTag="ArrayBufferView"
A.cu.$nativeSuperclassTag="ArrayBufferView"
A.cv.$nativeSuperclassTag="ArrayBufferView"
A.af.$nativeSuperclassTag="ArrayBufferView"})()
Function.prototype.$0=function(){return this()}
Function.prototype.$1=function(a){return this(a)}
Function.prototype.$2=function(a,b){return this(a,b)}
Function.prototype.$1$1=function(a){return this(a)}
Function.prototype.$3=function(a,b,c){return this(a,b,c)}
Function.prototype.$4=function(a,b,c,d){return this(a,b,c,d)}
convertAllToFastObject(w)
convertToFastObject($);(function(a){if(typeof document==="undefined"){a(null)
return}if(typeof document.currentScript!="undefined"){a(document.currentScript)
return}var s=document.scripts
function onLoad(b){for(var q=0;q<s.length;++q){s[q].removeEventListener("load",onLoad,false)}a(b.target)}for(var r=0;r<s.length;++r){s[r].addEventListener("load",onLoad,false)}})(function(a){v.currentScript=a
var s=A.nW
if(typeof dartMainRunner==="function"){dartMainRunner(s,[])}else{s([])}})})()
//# sourceMappingURL=main.dart.js.map
