(function () {
    "use strict";

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
        var sorts = [["hot", "🔥 Destacados"], ["new", "🆕 Nuevo"], ["top", "📈 Top"], ["comments", "💬 Comentados"]];
        var tabs = sorts.map(function (s) {
            return '<a href="?sort=' + s[0] + '" class="cvx-sort-btn ' + (s[0] === sort ? "cvx-sort-btn--active" : "") + '">' + s[1] + '</a>';
        }).join("");
        return '<div class="cvx-feed-header">'
            + tabs
            + '<a href="/communities/' + slug + '/create-post" class="cvx-new-post-btn">✏️ Nuevo post</a>'
            + '</div>';
    }

    function renderPostCard(post) {
        var scoreClass = post.Score > 0 ? "cvx-vote-score--pos" : post.Score < 0 ? "cvx-vote-score--neg" : "";
        return '<div class="cvx-post-card" onclick="window.location=\'/posts/' + post.Id + '\'">'
            + '<div class="cvx-post-vote">'
            + '<button class="cvx-vote-btn" onclick="event.stopPropagation();CorvaxDashboard.votePost(' + post.Id + ',1)">▲</button>'
            + '<div class="cvx-vote-score ' + scoreClass + '">' + post.Score + '</div>'
            + '<button class="cvx-vote-btn" onclick="event.stopPropagation();CorvaxDashboard.votePost(' + post.Id + ',-1)">▼</button>'
            + '</div>'
            + '<div class="cvx-post-body">'
            + '<div class="cvx-post-meta"><span class="cvx-post-author">' + (post.Username || "") + '</span></div>'
            + '<div class="cvx-post-title">' + (post.Title || "") + '</div>'
            + (post.ContentExcerpt ? '<div class="cvx-post-excerpt">' + post.ContentExcerpt + '</div>' : '')
            + '<div class="cvx-post-footer">'
            + '<a class="cvx-post-action" href="/posts/' + post.Id + '#comments" onclick="event.stopPropagation()">💬 ' + post.CommentCount + '</a>'
            + '</div>'
            + '</div>'
            + '</div>';
    }

    window.CorvaxDashboard = {
        votePost: function (postId, type) {
            fetch("/api/posts/" + postId + "/vote", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ voteType: type })    
            }).then(function (r) {
                if (r.status === 401) { window.location = "/Auth/Login"; return; }
                if (r.ok) window.location.reload();
            });
        }
    };

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", init);
    } else {
        init();
    }

})();