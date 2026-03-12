(function () {
    "use strict";
    window.CorvaxDashboard = {};
    function init() {
        var cfg = window.CORVAX;
        if (!cfg) return;

        var slug = cfg.slug;
        var params = new URLSearchParams(window.location.search);
        var sort = params.get("sort") || "hot";
        var page = parseInt(params.get("page") || "1", 10);

        loadPosts(slug, sort, page);

        var btn = document.getElementById("btn-follow");
        if (btn) {
            btn.addEventListener("click", function () {
                fetch("/api/communities/" + cfg.communityId + "/follow", { method: "POST" })
                    .then(function (r) {
                        if (r.status === 401) { window.location = "/Auth/Login"; return null; }
                        return r.json();
                    })
                    .then(function (data) {
                        if (!data) return;
                        btn.classList.toggle("is-following", data.isFollowing);
                        btn.textContent = data.isFollowing ? "✓ Siguiendo" : "+ Seguir";
                    });
            });
        }
    }

    function loadPosts(slug, sort, page) {
        var container = document.getElementById("post-feed-container");
        if (!container) return;

        container.innerHTML = '<div class="cvx-loading"><span class="cvx-spinner"></span> Cargando posts...</div>';

        fetch("/api/communities/" + encodeURIComponent(slug) + "/posts?sort=" + sort + "&page=" + page)
            .then(function (r) { return r.json(); })
            .then(function (data) {
                container.innerHTML = renderFeed(data, slug, sort, page);
            })
            .catch(function () {
                container.innerHTML = '<div class="cvx-empty-feed"><p>Error al cargar los posts.</p></div>';
            });
    }

    function renderFeed(data, slug, sort, page) {
        var items = data.Items || [];
        var pageSize = data.PageSize || 20;
        var totalCount = data.TotalCount || 0;
        var hasMore = page * pageSize < totalCount;

        if (!items.length) {
            return '<div class="cvx-empty-feed">'
                + '<div class="cvx-empty-feed__icon">📭</div>'
                + '<div class="cvx-empty-feed__title">Aún no hay posts</div>'
                + '<p>¡Sé el primero en publicar algo!</p>'
                + '</div>';
        }

        var html = renderFeedControls(slug, sort);
        items.forEach(function (post) { html += renderPostCard(post); });

        if (hasMore || page > 1) {
            html += '<div style="display:flex;justify-content:center;gap:10px;padding:20px 0;">';
            if (page > 1) html += '<a href="?sort=' + sort + '&page=' + (page - 1) + '" class="cvx-btn-outline">← Anterior</a>';
            if (hasMore) html += '<a href="?sort=' + sort + '&page=' + (page + 1) + '" class="cvx-btn-outline">Siguiente →</a>';
            html += '</div>';
        }

        return html;
    }

    function renderFeedControls(slug, sort) {
        var sorts = [
            ["hot", '<i class="fa-solid fa-fire"></i> Destacados'],
            ["new", '<i class="fa-solid fa-clock"></i> Nuevo'],
            ["top", '<i class="fa-solid fa-chart-line"></i> Top'],
            ["comments", '<i class="fa-regular fa-comment"></i> Comentados']
        ];
        var tabs = sorts.map(function (s) {
            return '<a href="?sort=' + s[0] + '" class="cvx-sort-btn ' + (s[0] === sort ? "cvx-sort-btn--active" : "") + '">' + s[1] + '</a>';
        }).join("");
        return '<div class="cvx-feed-header">'
            + tabs
            + '<button class="cvx-new-post-btn cvx-btn-outline" '
            + 'onclick="CorvaxDashboard.openCreatePost(\'' + slug + '\')">'
            + '<i class="fa-solid fa-pen"></i> Nuevo post</button>'
            + '</div>';
    }

    function renderPostCard(post) {

        const upActive = post.CurrentUserVote === 1 ? "is-active" : "";
        const downActive = post.CurrentUserVote === -1 ? "is-active" : "";
        const savedClass = post.IsSaved ? "is-saved" : "";

        // ── Avatar ────────────────────────────────────────────────────────────────
        // Igual que el Razor: img si hay UserAvatar, default con SVG de cuervo si no.
        const avatarHtml = post.UserAvatar
            ? `<img class="cvx-post-avatar" src="${post.UserAvatar}" alt="avatar">`
            : `<div class="cvx-post-avatar cvx-post-avatar-default">
           <svg class="cvx-icon-crow" viewBox="0 0 24 24" fill="currentColor">
               <path d="M21 3c-1.5 3-3.5 4-6 4-1.1 0-2.1-.2-3-.5
                        C10.5 5.5 9 4.5 7 4c-1.5-.4-3 0-4 1
                        1 .5 2.5.8 3.5 1.8C5.2 8 4.5 10 5 12
                        c.8 3.2 3.8 5 7 5 1 0 2-.2 3-.6
                        C16.8 18 19 16 20 13.5c.7-1.8.5-3.8-.5-5.5
                        C21 7 22 5 21 3z"/>
           </svg>
        </div>`;


        const excerptLen = (post.ContentExcerpt || "").length;
        const showVerMas = excerptLen > 120;


        // ── Tags / Categorías ─────────────────────────────────────────────────────
        // Tag principal con color CSS variable + hasta 2 categorías extra,
        // igual que el foreach con .Take(2) del Razor.
        let tagsHtml = "";

        if (post.MainCategoryName) {
            const color = post.MainCategoryColor || "#7c3aed";
            tagsHtml += `
                <span class="cvx-post-tag cvx-post-tag-main"
                      style="--tag-color:${color}">
                    <i class="fa-solid fa-tag"></i>
                    ${post.MainCategoryName}
                </span>`;
        }

        if (Array.isArray(post.ExtraCategories)) {
            post.ExtraCategories.slice(0, 2).forEach(function (cat) {
                tagsHtml += `<span class="cvx-post-tag">${cat}</span>`;
            });
        }

        // ── Fecha formateada ──────────────────────────────────────────────────────
        // El Razor usa ToString("g") → fecha + hora corta.
        // En JS lo replicamos con toLocaleString() que da el mismo resultado.
        const dateStr = post.CreatedAt
            ? new Date(post.CreatedAt).toLocaleString([], {
                year: "numeric",
                month: "numeric",
                day: "numeric",
                hour: "2-digit",
                minute: "2-digit"
            })
            : "";

        // ── Imagen ────────────────────────────────────────────────────────────────
        // El Razor usa <figure class="cvx-post-thumbnail">, igualamos eso.
        const imageHtml = post.Image
            ? `<figure class="cvx-post-thumbnail">
                   <img src="${post.Image}" loading="lazy" alt="">
               </figure>`
            : "";

        // ─────────────────────────────────────────────────────────────────────────
        return `
        <article class="cvx-post-card" data-post-id="${post.Id}">
 
            <!-- ── Columna de votos ────────────────────────────────────────── -->
            <aside class="cvx-post-vote">
 
                <button class="cvx-vote-btn cvx-vote-btn-up ${upActive}"
                        onclick="event.stopPropagation();CorvaxDashboard.votePost(${post.Id}, 1)">
                    <i class="fa-solid fa-chevron-up"></i>
                </button>
 
                <div class="cvx-vote-counters">
 
                    <div class="cvx-upvote-count">
                        ${renderVoteTier(post.UpVotes)}
                    </div>
 
                    <span class="cvx-vote-divider"></span>
 
                    <div class="cvx-downvote-count">
                        ${renderDownvoteTier(post.DownVotes)}
                    </div>
 
                </div>
 
                <button class="cvx-vote-btn cvx-vote-btn-down ${downActive}"
                        onclick="event.stopPropagation();CorvaxDashboard.votePost(${post.Id}, -1)">
                    <i class="fa-solid fa-chevron-down"></i>
                </button>
 
            </aside>
 
            <!-- ── Cuerpo del post ─────────────────────────────────────────── -->
            <div class="cvx-post-body">
 
                <!-- Meta: avatar + usuario + fecha + tags -->
                <header class="cvx-post-meta">
 
                    <div class="cvx-post-author">
                        ${avatarHtml}
                        <span class="cvx-post-username">${post.Username ?? ""}</span>
                    </div>
 
                    <span class="cvx-post-dot">•</span>
 
                    <time class="cvx-post-time">${dateStr}</time>
 
                    <div class="cvx-post-tags">
                        ${tagsHtml}
                    </div>
 
                </header>
 
                <!-- Título -->
                <h2 class="cvx-post-title">${post.Title ?? ""}</h2>
 
                    <!-- Excerpt con botón "Ver más" -->
                    <div class="cvx-post-preview">
                        <p class="cvx-post-excerpt">${post.ContentExcerpt ?? ""}</p>
                        ${showVerMas
                                    ? `<button class="cvx-expand-post"
                                       onclick="event.stopPropagation();CorvaxDashboard.expandPost(this)">
                                   <i class="fa-solid fa-chevron-down"></i> Ver más
                               </button>`
                                    : ""}
                    </div>
 
                <!-- Imagen (figure, igual que Razor) -->
                ${imageHtml}
 
                <!-- Footer de acciones -->
                <footer class="cvx-post-actions">
 
                    <!-- Comentarios: href sin #comments, igual que el Razor -->
                    <a class="cvx-post-action"
                       href="/posts/${post.Id}"
                       onclick="event.stopPropagation()">
                        <i class="fa-regular fa-comment"></i>
                        <span>${post.CommentCount ?? 0}</span>
                    </a>
 
                    <!-- Guardar -->
                    <button class="cvx-post-action cvx-action-save ${savedClass}"
                            onclick="event.stopPropagation();CorvaxDashboard.savePost(${post.Id}, this)">
                        <i class="fa-regular fa-bookmark"></i>
                        <span>Guardar</span>
                    </button>
 
                    <!-- Compartir -->
                    <button class="cvx-post-action cvx-action-share"
                            onclick="event.stopPropagation();CorvaxDashboard.sharePost(${post.Id})">
                        <i class="fa-solid fa-share-nodes"></i>
                        <span>Compartir</span>
                    </button>
 
                    <!-- Copiar link -->
                    <button class="cvx-post-action cvx-action-copy-link"
                            data-link="/posts/${post.Id}"
                            onclick="event.stopPropagation();CorvaxDashboard.copyLink(${post.Id})">
                        <i class="fa-solid fa-link"></i>
                    </button>
 
                </footer>
 
            </div>
 
        </article>`;
    }


    CorvaxDashboard.votePost = function (postId, type) {

        fetch("/api/posts/" + postId + "/vote", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ voteType: type })
        })
            .then(function (r) {
                if (r.status === 401) {
                    window.location = "/Auth/Login";
                    return null;
                }
                return r.json();
            })
            .then(function (data) {

                if (!data) return;

                const card = document.querySelector(`[data-post-id="${postId}"]`);
                if (!card) return;

                const upCount = card.querySelector(".cvx-upvote-count");
                const downCount = card.querySelector(".cvx-downvote-count");

                const upBtn = card.querySelector(".cvx-vote-btn-up");
                const downBtn = card.querySelector(".cvx-vote-btn-down");

                upCount.innerHTML = renderVoteTier(data.upVotes);
                downCount.innerHTML = renderDownvoteTier(data.downVotes);

                upBtn.classList.remove("is-active");
                downBtn.classList.remove("is-active");

                const vote = Number(data.currentUserVote);

                if (vote === 1) {
                    upBtn.classList.add("is-active");
                }

                if (vote === -1) {
                    downBtn.classList.add("is-active");
                }
            });
    };

    CorvaxDashboard.voteCommunity = function (communityId, voteType) {
        fetch("/Communities/VoteCommunity", {
            method: "POST",
            headers: { "Content-Type": "application/x-www-form-urlencoded" },
            body: "id=" + communityId + "&voteType=" + voteType
        })
            .then(function (r) {
                if (r.status === 401) { window.location = "/Auth/Login"; return null; }
                if (!r.ok) { console.error("VoteCommunity error HTTP:", r.status); return null; }
                return r.json();
            })
            .then(function (data) {
                if (!data) return;

                const widget = document.querySelector(".vote-widget[data-id='" + communityId + "']");
                if (!widget) { console.error("Widget no encontrado para id:", communityId); return; }

                const upBtn = widget.querySelector(".btn-vote.up");
                const downBtn = widget.querySelector(".btn-vote.down");
                const upCount = widget.querySelector(".up-counter");
                const downCount = widget.querySelector(".down-counter span");

                // leer ANTES de quitar la clase
                const wasActive = (voteType === 1 && upBtn.classList.contains("active"))
                    || (voteType === -1 && downBtn.classList.contains("active"));

                upBtn.classList.remove("active");
                downBtn.classList.remove("active");

                upCount.innerHTML = data.upVotes;
                if (downCount) downCount.textContent = data.downVotes;

                if (!wasActive) {
                    if (voteType === 1) upBtn.classList.add("active");
                    if (voteType === -1) downBtn.classList.add("active");
                }
            });
    };

    CorvaxDashboard.savePost = function (postId, btn) {
        fetch("/api/posts/" + postId + "/save", { method: "POST" })
            .then(function (r) {
                if (r.status === 401) { window.location = "/Auth/Login"; return null; }
                return r.json();
            })
            .then(function (data) {
                if (!data) return;
                btn.classList.toggle("is-saved", data.isSaved);
            });
    };

    CorvaxDashboard.expandPost = function (btn) {
        var preview = btn.closest(".cvx-post-preview");
        if (!preview) return;

        var isExpanded = preview.classList.contains("is-expanded");

        if (isExpanded) {
            preview.classList.remove("is-expanded");
            btn.innerHTML = '<i class="fa-solid fa-chevron-down"></i> Ver más';
        } else {
            preview.classList.add("is-expanded");
            btn.innerHTML = '<i class="fa-solid fa-chevron-up"></i> Ver menos';
        }
    };

    CorvaxDashboard.copyLink = function (postId) {

        const url = window.location.origin + "/posts/" + postId;

        navigator.clipboard.writeText(url).then(function () {
            console.log("Link copiado");
        });

    };

    CorvaxDashboard.openCreatePost = function (slug) {
        var existing = document.getElementById("cvx-create-post-modal");
        if (existing) existing.remove();

        // ── Bloquear scroll del fondo ───────────────────────────
        document.body.style.overflow = "hidden";

        // ── Overlay con blur ────────────────────────────────────
        var overlay = document.createElement("div");
        overlay.id = "cvx-create-post-modal";
        overlay.style.cssText = [
            "position:fixed", "inset:0", "z-index:9999",
            "background:rgba(7,7,15,0.72)",
            "backdrop-filter:blur(6px)",
            "-webkit-backdrop-filter:blur(6px)",
            "display:flex",
            "flex-direction:column",       // ← columna para label + box
            "align-items:center",
            "justify-content:center",
            "padding:20px"
        ].join(";");

        // ── Label "Nuevo post" ENCIMA de la ventana ─────────────
        var label = document.createElement("div");
        label.style.cssText = [
            "display:flex", "align-items:center", "gap:10px",
            "margin-bottom:10px",
            "font-family:'Barlow Condensed',sans-serif",
            "font-size:0.78rem",
            "font-weight:700",
            "letter-spacing:0.12em",
            "text-transform:uppercase",
            "color:rgba(255,255,255,0.45)",
            "user-select:none"
        ].join(";");
        label.innerHTML =
            '<span style="display:inline-block;width:6px;height:6px;border-radius:50%;background:#7c3aed;box-shadow:0 0 8px #7c3aed"></span>'
            + 'Nuevo post';

        // ── Contenedor del modal (sin header blanco) ────────────
        var box = document.createElement("div");
        box.style.cssText = [
            "background:#0d0d1a",
            "border:1px solid rgba(124,58,237,0.28)",
            "border-radius:14px",
            "width:min(720px,95vw)",
            "height:min(92vh,820px)",
            "display:flex",
            "flex-direction:column",
            "box-shadow:0 24px 64px rgba(0,0,0,0.8),0 0 0 1px rgba(255,255,255,0.03),0 0 40px rgba(124,58,237,0.12)",
            "overflow:hidden",
            "animation:cvxModalIn 0.22s cubic-bezier(0.22,1,0.36,1) both"
        ].join(";");

        // Inyectar keyframe de animación si no existe
        if (!document.getElementById("cvx-modal-keyframe")) {
            var style = document.createElement("style");
            style.id = "cvx-modal-keyframe";
            style.textContent = [
                "@keyframes cvxModalIn {",
                "  from { opacity:0; transform:scale(0.94) translateY(12px); }",
                "  to   { opacity:1; transform:scale(1)    translateY(0); }",
                "}"
            ].join("");
            document.head.appendChild(style);
        }

        // ── Iframe apunta al Razor sin header blanco ────────────
        var iframe = document.createElement("iframe");
        iframe.src = "/communities/" + slug + "/create-post?modal=1";
        iframe.style.cssText = "flex:1;border:none;width:100%;border-radius:14px;";
        iframe.name = "cvx-create-frame";

        box.appendChild(iframe);

        // ── Click en overlay (fuera del label y box) cierra ─────
        overlay.addEventListener("click", function (e) {
            if (e.target === overlay) CorvaxDashboard.closeCreatePost();
        });

        overlay.appendChild(label);
        overlay.appendChild(box);
        document.body.appendChild(overlay);
    };
    // Esta función la llama el iframe desde adentro al terminar
    CorvaxDashboard.closeCreatePost = function () {
        document.body.style.overflow = ""; 
        var modal = document.getElementById("cvx-create-post-modal");
        if (modal) modal.remove();
        // Recarga el feed sin recargar la página
        var cfg = window.CORVAX;
        if (cfg) loadPosts(cfg.slug, "new", 1);
    };


            function renderVoteTier(votes) {
                votes = votes ?? 0;

                const TIER_SIZE = 700;
                const SUBTIER_SIZE = TIER_SIZE / 5;   // 140

                const tiers = [
                    "tier-bronce",
                    "tier-plata",
                    "tier-oro",
                    "tier-platino",
                    "tier-diamante",
                    "tier-amatista",
                    "tier-ebano",
                    "tier-corvus"
                ];

                const names = [
                    ["Polluelo Curioso", "Polluelo Observador", "Polluelo Explorador", "Polluelo Vigilante", "Polluelo del Nido"],
                    ["Pluma Plateada I", "Pluma Plateada II", "Pluma Plateada III", "Pluma Plateada IV", "Pluma Plateada V"],
                    ["Cuervo Iniciado", "Cuervo Mensajero", "Cuervo Vigilante", "Cuervo Estratega", "Cuervo del Ocaso"],
                    ["Nido de Piedra", "Nido Elevado", "Nido Guardián", "Nido Arcano", "Nido Supremo"],
                    ["Bandada Errante", "Bandada Vigía", "Bandada Sombría", "Bandada Imperial", "Bandada Ancestral"],
                    ["Heraldo del Viento", "Señor del Viento", "Maestro del Vuelo", "Oráculo del Cielo", "Soberano de Alas"],
                    ["Guardián de Corvax", "Portador del Cuervo", "Vigía del Abismo", "Señor de la Bandada", "Mano de Corvax"],
                    ["Elegido de Corvax", "Heraldo de Corvax", "Avatar de Corvax", "Sombra de Corvax", "Corona de Corvax"]
                ];

                const icons = [
                    "fa-feather",
                    "fa-feather",
                    "fa-crow",
                    "fa-egg",
                    "fa-feather-pointed",
                    "fa-wind",
                    "fa-skull",
                    "fa-crown"
                ];

                // ── Cálculo de tier ───────────────────────────────────────────
                let tierIndex = Math.min(7, Math.floor(votes / TIER_SIZE));
                let subTier = Math.min(4, Math.floor((votes % TIER_SIZE) / SUBTIER_SIZE));

                let tier = tiers[tierIndex];
                let tierName = names[tierIndex][subTier];
                let icon = icons[tierIndex];

                // Progreso relativo al inicio del tier actual (igual que el Razor)
                const currentTierStart = tierIndex * TIER_SIZE;
                const nextRank = currentTierStart + TIER_SIZE;
                const progressPercent = Math.min(100, ((votes - currentTierStart) / TIER_SIZE) * 100);

                // ── Easter eggs (sincronizados con _VoteTier.cshtml) ──────────
                let specialClass = "";

                if (votes === 69) {
                    tierName = "Nice.";
                    icon = "fa-face-smile";
                    specialClass = "rank-meme";
                } else if (votes === 404) {
                    tierName = "Rank Not Found";
                    icon = "fa-bug";
                    specialClass = "rank-error";
                } else if (votes === 666) {
                    tier = "tier-ebano";
                    tierName = "Cuervo del Abismo";
                    icon = "fa-skull";
                    specialClass = "rank-abyss";
                } else if (votes === 777) {
                    tier = "tier-corvus";
                    tierName = "Reliquia de Corvax";
                    icon = "fa-crown";
                    specialClass = "rank-relic";
                } else if (votes === 999) {
                    tier = "tier-amatista";
                    tierName = "Oráculo del Vacío";
                    icon = "fa-eye";
                    specialClass = "rank-oracle";
                } else if (votes === 1337) {
                    tier = "tier-ebano";
                    tierName = "Hack del Cuervo";
                    icon = "fa-terminal";
                    specialClass = "rank-hacker";
                } else if (votes === 7777) {
                    tier = "tier-corvus";
                    tierName = "Trono de Corvax";
                    icon = "fa-crown";
                    specialClass = "rank-throne";
                } else if (votes >= 10000) {
                    tier = "tier-corvus";
                    tierName = "Artefacto Ancestral";
                    icon = "fa-gem";
                    specialClass = "rank-artifact";
                }

                const viral = votes >= 2500 ? "post-viral" : "";

                return `
        <div class="vote-tier tooltip-container ${tier} ${specialClass} ${viral}">

            <span class="vote-number">${votes}</span>

            <div class="vote-tier-tooltip">

                <div class="tier-header">
                    <i class="fa-solid ${icon}"></i>
                    <span class="tier-name">${tierName}</span>
                </div>

                <div class="tier-progress-bar">
                    <div class="tier-progress-fill" style="width:${progressPercent}%"></div>
                </div>

                <small class="tier-progress-text">
                    ${votes - currentTierStart} / ${TIER_SIZE} para el siguiente rango
                </small>

            </div>

        </div>`;
            }

            function renderDownvoteTier(votes) {
                votes = votes ?? 0;

                const tiers = [
                    "dv-tier-murmur",
                    "dv-tier-shadow",
                    "dv-tier-corrupt",
                    "dv-tier-ruin",
                    "dv-tier-void",
                    "dv-tier-curse",
                    "dv-tier-entity",
                    "dv-tier-abyss"
                ];

                const names = [
                    ["Murmullo del Vacío", "Pluma Inquieta", "Eco Desentonado", "Tinta Derramada", "Nota Desafinada"],
                    ["Sombra Ligera", "Pluma Marchita", "Susurro Persistente", "Rastro Oscuro", "Eco Torcido"],
                    ["Tinta Corrupta", "Archivo Distorsionado", "Fragmento Inestable", "Pluma Profanada", "Eco del Abismo"],
                    ["Texto en Ruinas", "Debate Colapsado", "Fragmento Caído", "Página Quemada", "Eco Devastado"],
                    ["Señal Perdida", "Registro Maldito", "Nido Profanado", "Archivo Prohibido", "Eco del Vacío"],
                    ["Sello de Silencio", "Registro Condenado", "Herida del Foro", "Marca del Abismo", "Maldición Persistente"],
                    ["Sombras Antiguas", "Vigía del Vacío", "Oráculo Caído", "Guardián Roto", "Portador del Eco"],
                    ["Fragmento del Abismo", "Voz del Vacío", "Archivo Maldito", "Sombra de Corvax", "Reliquia del Abismo"]
                ];

                const icons = [
                    "fa-comment-slash",
                    "fa-cloud",
                    "fa-skull",
                    "fa-scroll",
                    "fa-circle-xmark",
                    "fa-skull-crossbones",
                    "fa-ghost",
                    "fa-crow"
                ];

                // ── Lógica de tier ───────────────────────────────────────
                let tierIndex = Math.min(7, Math.floor(votes / 700));
                let subTier = Math.min(4, Math.floor((votes % 700) / 140));

                let tier = tiers[tierIndex];
                let tierName = names[tierIndex][subTier];
                let icon = icons[tierIndex];

                let nextRank = (tierIndex + 1) * 700;
                let progressPercent = Math.min(100, (votes / nextRank) * 100);

                // ── Easter eggs ──────────────────────────────────────────
                let specialClass = "";
                let hidden = false;

                if (votes === 13) {
                    tierName = "Presagio"; icon = "fa-eye"; specialClass = "dv-omen"; hidden = true;
                } else if (votes === 66) {
                    tierName = "Eco Infernal"; icon = "fa-fire"; specialClass = "dv-hell"; hidden = true;
                } else if (votes === 404) {
                    tierName = "Post No Encontrado"; icon = "fa-ghost"; specialClass = "dv-error"; hidden = true;
                } else if (votes === 666) {
                    tier = "dv-tier-abyss";
                    tierName = "Maldición Mayor"; icon = "fa-skull"; specialClass = "dv-demon"; hidden = true;
                } else if (votes === 777) {
                    tierName = "Error del Oráculo"; icon = "fa-bug"; specialClass = "dv-glitch"; hidden = true;
                } else if (votes === 999) {
                    tierName = "Fin del Ciclo"; icon = "fa-hourglass-end"; specialClass = "dv-cycle"; hidden = true;
                } else if (votes === 1337) {
                    tierName = "Caos Técnico"; icon = "fa-terminal"; specialClass = "dv-hacker"; hidden = true;
                }

                const anomaly = votes >= 500;

                // ── Tooltip extra: anomalía ──────────────────────────────
                const anomalyTag = anomaly
                    ? `<small class="dv-anomaly-label">⚠ Anomalía detectada</small>`
                    : "";

                return `
        <div class="dv-vote-tier tooltip-container ${tier} ${specialClass}">

            <span class="dv-vote-number">-${votes}</span>

            <div class="dv-vote-tooltip">

                <div class="dv-tier-header">
                    <i class="fa-solid ${icon}"></i>
                    <span class="dv-tier-name">${tierName}</span>
                </div>

                <div class="dv-tier-progress-bar">
                    <div class="dv-tier-progress-fill" style="width:${progressPercent}%"></div>
                </div>

                <small class="dv-tier-progress-text">
                    ${votes} / ${nextRank} votos negativos
                </small>

                ${anomalyTag}

            </div>

        </div>`;
            }


    CorvaxDashboard.loadPosts = loadPosts;

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", init);
    } else {
        init();
    }

})();