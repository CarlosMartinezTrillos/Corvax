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
            + '<a href="/communities/' + slug + '/create-post" class="cvx-new-post-btn cvx-btn-outline">'
            + '<i class="fa-solid fa-pen"></i> Nuevo post</a>'
            + '</div>';
    }

        function renderPostCard(post) {

            const upActive = post.CurrentUserVote === 1 ? "is-active" : "";
            const downActive = post.CurrentUserVote === -1 ? "is-active" : "";

                    return `
            <article class="cvx-post-card" data-post-id="${post.Id}">

                <aside class="cvx-post-vote">

                    <button class="cvx-vote-btn cvx-vote-btn-up ${upActive}"
                        onclick="event.stopPropagation();CorvaxDashboard.votePost(${post.Id},1)">

                        <i class="fa-solid fa-chevron-up"></i>

                    </button>

                    <div class="cvx-vote-counters">

                        <span class="cvx-upvote-count">
                            ${post.UpVotes ?? 0}
                        </span>

                        <span class="cvx-vote-divider"></span>

                        <span class="cvx-downvote-count">
                            ${post.DownVotes ?? 0}
                        </span>

                    </div>

                    <button class="cvx-vote-btn cvx-vote-btn-down ${downActive}"
                        onclick="event.stopPropagation();CorvaxDashboard.votePost(${post.Id},-1)">

                        <i class="fa-solid fa-chevron-down"></i>

                    </button>

                </aside>


                <div class="cvx-post-body">

                    <header class="cvx-post-meta">

                        <span class="cvx-post-author">
                            ${post.Username ?? ""}
                        </span>

                        <span class="cvx-post-dot">•</span>

                        <span class="cvx-post-time">
                           ${post.CreatedAt ? new Date(post.CreatedAt).toLocaleDateString() : ""}
                        </span>

                    </header>


                    <h2 class="cvx-post-title">
                        ${post.Title ?? ""}
                    </h2>


                    ${post.ContentExcerpt ?
                            `<p class="cvx-post-excerpt">${post.ContentExcerpt}</p>`
                            : ""
                        }


                    ${post.Image ?
                            `<div class="cvx-post-image">
                            <img src="${post.Image}" loading="lazy" />
                    </div>`
                            : ""
                        }


                    <footer class="cvx-post-footer">

                        <a class="cvx-post-action"
                           href="/posts/${post.Id}#comments"
                           onclick="event.stopPropagation()">

                           <i class="fa-regular fa-comment"></i>
                           ${post.CommentCount ?? 0}

                        </a>

                            <button class="cvx-post-action"
                                onclick="event.stopPropagation();CorvaxDashboard.copyLink(${post.Id})">

                                <i class="fa-solid fa-link"></i>

                            </button>

                    </footer>

                </div>

            </article>
            `;
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

                upCount.textContent = data.upVotes;
                downCount.textContent = data.downVotes;

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

    CorvaxDashboard.copyLink = function (postId) {

        const url = window.location.origin + "/posts/" + postId;

        navigator.clipboard.writeText(url).then(function () {
            console.log("Link copiado");
        });

    };

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", init);
    } else {
        init();
    }

})();