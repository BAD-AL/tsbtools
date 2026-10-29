// TSB3 player photos: 85 pictures of 24x32 pixels in the ROM's 16 grays. The ROM is the source of truth (the WebHelper bridge reads and
// writes it); this module keeps a copy to draw from.

export const cache = { on: false, palette: [], pixels: [], owners: [], urls: [], bySlot: new Map() };

export const PHOTO_W = 24, PHOTO_H = 32, PLAYER_PHOTOS = 84, SILHOUETTE = 84;
export const hex = i => '0x' + i.toString(16).toUpperCase().padStart(2, '0');

function makeUrl(px) {
  const c = document.createElement('canvas');
  c.width = PHOTO_W; c.height = PHOTO_H;
  const ctx = c.getContext('2d'), img = ctx.createImageData(PHOTO_W, PHOTO_H);
  for (let i = 0; i < px.length; i++) {
    const col = cache.palette[px[i]] || [0, 0, 0];
    img.data[i * 4] = col[0]; img.data[i * 4 + 1] = col[1]; img.data[i * 4 + 2] = col[2];
    img.data[i * 4 + 3] = px[i] === 0 ? 0 : 255;          // palette entry 0 is transparent
  }
  ctx.putImageData(img, 0, 0);
  return c.toDataURL();
}

export function refreshOwners(web) {
  cache.owners = web.GetPhotoOwners().split('\n').slice(0, PLAYER_PHOTOS + 1);
  cache.bySlot = new Map();
  cache.owners.forEach((o, i) => { if (i < PLAYER_PHOTOS && o.includes('|') && !cache.bySlot.has(o)) cache.bySlot.set(o, i); });   // the first entry wins, as in the game
}

/** Reads every photo from the ROM (call after a ROM loads). */
export function load(web) {
  cache.on = false;
  if (!web.HasPhotos()) return;
  cache.palette = web.GetPhotoPalette().split(',').map(h => [0, 2, 4].map(i => parseInt(h.substr(i, 2), 16)));
  cache.pixels = []; cache.urls = [];
  for (let i = 0; i <= PLAYER_PHOTOS; i++) { cache.pixels[i] = web.GetPhotoPixels(i); cache.urls[i] = makeUrl(cache.pixels[i]); }
  refreshOwners(web);
  cache.on = true;
}

/** Re-reads one photo's picture and who owns what (after a change). */
export function refresh(web, photo) {
  cache.pixels[photo] = web.GetPhotoPixels(photo);
  cache.urls[photo] = makeUrl(cache.pixels[photo]);
  refreshOwners(web);
}

/** The photo number of a roster slot, or undefined (the silhouette). */
export const photoOf = (team, pos) => cache.bySlot.get(team + '|' + pos);

/** Draws 768 palette indexes on a canvas at a whole-number scale. */
export function draw(canvas, px, scale) {
  canvas.width = PHOTO_W * scale; canvas.height = PHOTO_H * scale;
  const ctx = canvas.getContext('2d');
  for (let y = 0; y < PHOTO_H; y++) {
    for (let x = 0; x < PHOTO_W; x++) {
      const col = cache.palette[px[y * PHOTO_W + x]] || [0, 0, 0];
      ctx.fillStyle = `rgb(${col[0]},${col[1]},${col[2]})`;
      ctx.fillRect(x * scale, y * scale, scale, scale);
    }
  }
}

/** An image file as ARGB ints (large images are scaled down first; 24x32 stays as it is). */
export async function readImage(file, maxSide = 480) {
  const bmp = await createImageBitmap(file);
  let w = bmp.width, h = bmp.height;
  if (!(w === PHOTO_W && h === PHOTO_H)) {
    const k = Math.min(1, maxSide / Math.max(w, h));
    w = Math.max(1, Math.round(w * k)); h = Math.max(1, Math.round(h * k));
  }
  const canvas = document.createElement('canvas');
  canvas.width = w; canvas.height = h;
  const ctx = canvas.getContext('2d');
  ctx.imageSmoothingQuality = 'high';
  ctx.drawImage(bmp, 0, 0, w, h);
  const d = ctx.getImageData(0, 0, w, h).data, argb = new Int32Array(w * h);
  for (let i = 0; i < argb.length; i++) argb[i] = (d[i * 4 + 3] << 24) | (d[i * 4] << 16) | (d[i * 4 + 1] << 8) | d[i * 4 + 2];
  return { w, h, argb, canvas };
}
